#nullable enable
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(
        IOptions<FileStorageSettings> settings,
        ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var blobSettings = settings.Value.AzureBlob;
        var blobServiceClient = new BlobServiceClient(blobSettings.ConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(blobSettings.ContainerName);
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

        return new FileDownloadResult(
            Content: download.Value.Content,
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
