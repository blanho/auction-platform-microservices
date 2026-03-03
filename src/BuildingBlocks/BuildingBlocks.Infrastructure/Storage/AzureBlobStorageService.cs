#nullable enable
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;

namespace BuildingBlocks.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _connectionString;
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        IOptions<FileStorageSettings> settings,
        RecyclableMemoryStreamManager streamManager,
        ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        _streamManager = streamManager;
        var blobSettings = settings.Value.AzureBlob;
        _connectionString = blobSettings.ConnectionString;
        _blobServiceClient = new BlobServiceClient(_connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(blobSettings.ContainerName);
    }

    public async Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var fileId = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var storedFileName = $"{fileId}{extension}";

        var subFolder = request.SubFolder ?? DateTime.UtcNow.ToString("yyyy/MM");
        var blobPath = $"{subFolder}/{storedFileName}";

        var blobClient = _containerClient.GetBlobClient(blobPath);

        var headers = new BlobHttpHeaders { ContentType = request.ContentType };
        var metadata = request.Metadata ?? new Dictionary<string, string>();
        metadata["OriginalFileName"] = request.FileName;

        if (request.OwnerId.HasValue)
        {
            metadata["OwnerId"] = request.OwnerId.Value.ToString();
        }

        await blobClient.UploadAsync(
            request.Content,
            new BlobUploadOptions
            {
                HttpHeaders = headers,
                Metadata = metadata
            },
            cancellationToken);

        _logger.LogInformation("File uploaded to Azure Blob: {FileId} -> {BlobPath}", fileId, blobPath);

        return new FileUploadResult(
            FileId: fileId,
            FileName: request.FileName,
            StoredFileName: storedFileName,
            ContentType: request.ContentType,
            FileSize: request.FileSize,
            Url: blobClient.Uri.ToString(),
            UploadedAt: DateTimeOffset.UtcNow
        );
    }

    public async Task<FileDownloadResult?> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var blobClient = await FindBlobAsync(fileId, cancellationToken);
        if (blobClient is null)
        {
            return null;
        }

        var download = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        var properties = download.Value.Details;

        var memoryStream = _streamManager.GetStream(tag: fileId);
        await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return new FileDownloadResult(
            Content: memoryStream,
            FileName: properties.Metadata.TryGetValue("OriginalFileName", out var originalName) ? originalName : Path.GetFileName(blobClient.Name),
            ContentType: properties.ContentType,
            FileSize: properties.ContentLength
        );
    }

    public async Task<string?> GetUrlAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var blobClient = await FindBlobAsync(fileId, cancellationToken);
        return blobClient?.Uri.ToString();
    }

    public async Task<bool> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var blobClient = await FindBlobAsync(fileId, cancellationToken);
        if (blobClient is null)
        {
            return false;
        }

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("File deleted from Azure Blob: {FileId}", fileId);
        return true;
    }

    public async Task<bool> ExistsAsync(string fileId, CancellationToken cancellationToken = default)
    {
        var blobClient = await FindBlobAsync(fileId, cancellationToken);
        return blobClient is not null;
    }

    public async Task<PresignedUploadResult?> GenerateUploadSasTokenAsync(
        PresignedUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var fileId = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var storedFileName = $"{fileId}{extension}";
        var subFolder = request.SubFolder ?? DateTime.UtcNow.ToString("yyyy/MM");
        var blobPath = $"{subFolder}/{storedFileName}";

        var blobClient = _containerClient.GetBlobClient(blobPath);
        var credential = ParseStorageSharedKeyCredential(_connectionString);

        if (credential is null)
        {
            _logger.LogWarning("Cannot generate SAS token — connection string does not contain account key");
            return null;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = expiresAt
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
        var uploadUrl = $"{blobClient.Uri}?{sasToken}";

        var headers = new Dictionary<string, string>
        {
            ["x-ms-blob-type"] = "BlockBlob",
            ["Content-Type"] = request.ContentType
        };

        _logger.LogInformation("Generated upload SAS token for blob: {BlobPath}", blobPath);

        return new PresignedUploadResult(
            FileId: fileId,
            StoredFileName: storedFileName,
            UploadUrl: uploadUrl,
            Headers: headers,
            ExpiresAt: expiresAt
        );
    }

    public async Task<PresignedDownloadResult?> GenerateDownloadSasTokenAsync(
        string fileId,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var blobClient = await FindBlobAsync(fileId, cancellationToken);
        if (blobClient is null)
        {
            return null;
        }

        var credential = ParseStorageSharedKeyCredential(_connectionString);
        if (credential is null)
        {
            _logger.LogWarning("Cannot generate SAS token — connection string does not contain account key");
            return null;
        }

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        var expiresAt = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1));

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = expiresAt
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
        var downloadUrl = $"{blobClient.Uri}?{sasToken}";

        var originalName = properties.Value.Metadata.TryGetValue("OriginalFileName", out var name)
            ? name
            : Path.GetFileName(blobClient.Name);

        _logger.LogInformation("Generated download SAS token for file: {FileId}", fileId);

        return new PresignedDownloadResult(
            DownloadUrl: downloadUrl,
            FileName: originalName,
            ContentType: properties.Value.ContentType,
            ExpiresAt: expiresAt
        );
    }

    private static StorageSharedKeyCredential? ParseStorageSharedKeyCredential(string connectionString)
    {
        var parts = connectionString.Split(';')
            .Select(s => s.Split('=', 2))
            .Where(s => s.Length == 2)
            .ToDictionary(s => s[0].Trim(), s => s[1].Trim(), StringComparer.OrdinalIgnoreCase);

        if (parts.TryGetValue("AccountName", out var accountName) &&
            parts.TryGetValue("AccountKey", out var accountKey))
        {
            return new StorageSharedKeyCredential(accountName, accountKey);
        }

        return null;
    }

    private async Task<BlobClient?> FindBlobAsync(string fileId, CancellationToken cancellationToken)
    {
        await foreach (var blob in _containerClient.GetBlobsAsync(prefix: "", cancellationToken: cancellationToken))
        {
            if (Path.GetFileNameWithoutExtension(blob.Name) == fileId)
            {
                return _containerClient.GetBlobClient(blob.Name);
            }
        }

        return null;
    }
}
