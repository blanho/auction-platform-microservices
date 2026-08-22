#nullable enable
using Azure.Identity;
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
    private readonly StorageSharedKeyCredential? _sharedKeyCredential;
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
        if (!string.IsNullOrWhiteSpace(blobSettings.ConnectionString))
        {
            _blobServiceClient = new BlobServiceClient(blobSettings.ConnectionString);
            _sharedKeyCredential = ParseStorageSharedKeyCredential(blobSettings.ConnectionString);
        }
        else if (Uri.TryCreate(blobSettings.ServiceUri, UriKind.Absolute, out var serviceUri))
        {
            _blobServiceClient = new BlobServiceClient(serviceUri, new DefaultAzureCredential());
        }
        else
        {
            throw new InvalidOperationException(
                "Azure Blob storage requires FileStorage:AzureBlob:ConnectionString " +
                "or FileStorage:AzureBlob:ServiceUri.");
        }

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
        metadata[BlobStorageConstants.MetadataKeys.OriginalFileName] = request.FileName;

        if (request.OwnerId.HasValue)
        {
            metadata[BlobStorageConstants.MetadataKeys.OwnerId] = request.OwnerId.Value.ToString();
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
            StoredFileName: blobPath,
            ContentType: request.ContentType,
            FileSize: request.FileSize,
            Url: blobClient.Uri.ToString(),
            UploadedAt: DateTimeOffset.UtcNow
        );
    }

    public async Task<FileDownloadResult?> DownloadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        var exists = await blobClient.ExistsAsync(cancellationToken);
        if (!exists.Value)
        {
            return null;
        }

        var download = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        var properties = download.Value.Details;

        var memoryStream = _streamManager.GetStream(tag: storedFileName);
        await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return new FileDownloadResult(
            Content: memoryStream,
            FileName: properties.Metadata.TryGetValue(BlobStorageConstants.MetadataKeys.OriginalFileName, out var originalName) ? originalName : Path.GetFileName(blobClient.Name),
            ContentType: properties.ContentType,
            FileSize: properties.ContentLength
        );
    }

    public async Task<string?> GetUrlAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        var exists = await blobClient.ExistsAsync(cancellationToken);
        return exists.Value ? blobClient.Uri.ToString() : null;
    }

    public async Task<bool> DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        if (response.Value)
        {
            _logger.LogInformation("File deleted from Azure Blob: {StoredFileName}", storedFileName);
        }

        return response.Value;
    }

    public async Task<bool> ExistsAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        var exists = await blobClient.ExistsAsync(cancellationToken);
        return exists.Value;
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
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobPath,
            Resource = BlobStorageConstants.BlobResource,
            ExpiresOn = expiresAt
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sasToken = await GenerateSasQueryAsync(sasBuilder, expiresAt, cancellationToken);
        var uploadUrl = $"{blobClient.Uri}?{sasToken}";

        var headers = new Dictionary<string, string>
        {
            [BlobStorageConstants.Headers.BlobType] = BlobStorageConstants.BlockBlob,
            [BlobStorageConstants.Headers.ContentType] = request.ContentType
        };

        _logger.LogInformation("Generated upload SAS token for blob: {BlobPath}", blobPath);

        return new PresignedUploadResult(
            FileId: fileId,
            StoredFileName: blobPath,
            UploadUrl: uploadUrl,
            Headers: headers,
            ExpiresAt: expiresAt
        );
    }

    public async Task<PresignedDownloadResult?> GenerateDownloadSasTokenAsync(
        string storedFileName,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(storedFileName);
        var exists = await blobClient.ExistsAsync(cancellationToken);
        if (!exists.Value)
        {
            return null;
        }

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
        var expiresAt = DateTimeOffset.UtcNow.Add(expiry ?? TimeSpan.FromHours(1));

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobClient.Name,
            Resource = BlobStorageConstants.BlobResource,
            ExpiresOn = expiresAt
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = await GenerateSasQueryAsync(sasBuilder, expiresAt, cancellationToken);
        var downloadUrl = $"{blobClient.Uri}?{sasToken}";

        var originalName = properties.Value.Metadata.TryGetValue(BlobStorageConstants.MetadataKeys.OriginalFileName, out var name)
            ? name
            : Path.GetFileName(blobClient.Name);

        _logger.LogInformation("Generated download SAS token for file: {StoredFileName}", storedFileName);

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

    private async Task<string> GenerateSasQueryAsync(
        BlobSasBuilder sasBuilder,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        if (_sharedKeyCredential is not null)
        {
            return sasBuilder.ToSasQueryParameters(_sharedKeyCredential).ToString();
        }

        var startsAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        sasBuilder.StartsOn = startsAt;
        var delegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(
            startsAt,
            expiresAt,
            cancellationToken);

        return sasBuilder
            .ToSasQueryParameters(delegationKey.Value, _blobServiceClient.AccountName)
            .ToString();
    }

}
