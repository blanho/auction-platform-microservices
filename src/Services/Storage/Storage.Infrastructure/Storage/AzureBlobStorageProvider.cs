using System.Security.Cryptography;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Storage.Application.Configuration;
using Storage.Application.DTOs;
using Storage.Application.Interfaces;
using Storage.Domain.Constants;

namespace Storage.Infrastructure.Storage;

public class AzureBlobStorageProvider : IStorageProvider, IAzureBlobStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly StorageOptions _options;
    private readonly ILogger<AzureBlobStorageProvider> _logger;

    public AzureBlobStorageProvider(
        BlobServiceClient blobServiceClient,
        IOptions<StorageOptions> options,
        ILogger<AzureBlobStorageProvider> logger)
    {
        _blobServiceClient = blobServiceClient;
        _options = options.Value;
        _logger = logger;
    }

    #region IStorageProvider Implementation

    public async Task<StorageUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blobPath = GetStoragePath(folder, fileName);
            var containerName = GetContainerName(folder);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobPath);

            stream.Position = 0;

            var checksum = await ComputeStreamChecksumAsync(stream, cancellationToken);
            stream.Position = 0;

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "private, max-age=31536000"
            };

            var metadata = new Dictionary<string, string>
            {
                ["original-filename"] = fileName,
                ["content-hash"] = checksum,
                ["uploaded-at"] = DateTimeOffset.UtcNow.ToString("O")
            };

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = headers,
                    Metadata = metadata
                },
                cancellationToken);

            _logger.LogInformation(
                "Uploaded blob {BlobPath} to container {Container} ({Size} bytes)",
                blobPath, containerName, stream.Length);

            return new StorageUploadResult(
                Success: true,
                Path: blobPath,
                Url: blobClient.Uri.ToString(),
                Checksum: checksum);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to upload blob {FileName}", fileName);
            return new StorageUploadResult(
                Success: false,
                Error: ex.Message);
        }
    }

    public async Task<Stream?> DownloadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobPath) = ParsePath(path);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Blob not found: {Path}", path);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobPath) = ParsePath(path);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var response = await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots,
                cancellationToken: cancellationToken);

            if (response.Value)
            {
                _logger.LogInformation("Deleted blob: {Path}", path);
            }

            return response.Value;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to delete blob: {Path}", path);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobPath) = ParsePath(path);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            return await blobClient.ExistsAsync(cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Failed to check blob existence: {Path}", path);
            return false;
        }
    }

    public async Task<StorageFileInfo?> GetFileInfoAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobPath) = ParsePath(path);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            return new StorageFileInfo
            {
                Path = path,
                ContentType = properties.Value.ContentType,
                Size = properties.Value.ContentLength,
                LastModified = properties.Value.LastModified,
                ContentHash = properties.Value.ContentHash != null
                    ? Convert.ToBase64String(properties.Value.ContentHash)
                    : null,
                Metadata = properties.Value.Metadata.ToDictionary(x => x.Key, x => x.Value)
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<bool> CopyAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (sourceContainer, sourceBlob) = ParsePath(sourcePath);
            var (destContainer, destBlob) = ParsePath(destinationPath);

            var sourceContainerClient = _blobServiceClient.GetBlobContainerClient(sourceContainer);
            var sourceBlobClient = sourceContainerClient.GetBlobClient(sourceBlob);

            var destContainerClient = _blobServiceClient.GetBlobContainerClient(destContainer);
            await destContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            var destBlobClient = destContainerClient.GetBlobClient(destBlob);

            var copyOperation = await destBlobClient.StartCopyFromUriAsync(
                sourceBlobClient.Uri,
                cancellationToken: cancellationToken);

            await copyOperation.WaitForCompletionAsync(cancellationToken);

            _logger.LogInformation(
                "Copied blob from {Source} to {Destination}",
                sourcePath, destinationPath);

            return true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to copy blob from {Source} to {Destination}",
                sourcePath, destinationPath);
            return false;
        }
    }

    public async Task<string?> MoveAsync(
        string sourcePath,
        string destinationFolder,
        CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = GetStoragePath(destinationFolder, fileName);

        var copied = await CopyAsync(sourcePath, destinationPath, cancellationToken);
        if (copied)
        {
            await DeleteAsync(sourcePath, cancellationToken);
            return destinationPath;
        }
        return null;
    }

    public async Task<PreSignedUrlResult> GenerateUploadUrlAsync(
        string fileName,
        string contentType,
        string folder,
        int expirationMinutes = 15,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return GeneratePresignedUploadUrl(fileName, contentType, folder, expirationMinutes);
    }

    public async Task<PreSignedUrlResult> GenerateDownloadUrlAsync(
        string path,
        int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return GeneratePresignedDownloadUrl(path, null, expirationMinutes);
    }

    public async Task<string?> ComputeChecksumAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobPath) = ParsePath(path);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            if (properties.Value.ContentHash != null)
            {
                return Convert.ToBase64String(properties.Value.ContentHash);
            }

            using var stream = await DownloadAsync(path, cancellationToken);
            if (stream != null)
            {
                return await ComputeStreamChecksumAsync(stream, cancellationToken);
            }

            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex, "Failed to compute checksum for blob: {Path}", path);
            return null;
        }
    }

    public string GetStoragePath(string folder, string fileName)
    {

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";

        Guid fileId;
        if (!Guid.TryParse(fileNameWithoutExt, out fileId))
        {
            fileId = Guid.NewGuid();
        }

        var idStr = fileId.ToString("N").ToLower();
        var prefix = idStr.Substring(0, 2);

        return $"{folder}/{prefix}/{idStr}{extension}";
    }

    #endregion

    #region Presigned URL Operations

    public PreSignedUrlResult GeneratePresignedUploadUrl(
        string fileName,
        string contentType,
        string folder,
        int expirationMinutes = 15)
    {
        var blobPath = GetStoragePath(folder, fileName);
        var containerName = GetContainerName(folder);
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        _logger.LogInformation(
            "Generated upload SAS URL for {BlobPath} with {Expiry}min expiry",
            blobPath, expirationMinutes);

        return new PreSignedUrlResult(
            Success: true,
            Url: sasUri.ToString(),
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(expirationMinutes),
            RequiredHeaders: new Dictionary<string, string>
            {
                ["x-ms-blob-type"] = "BlockBlob",
                ["Content-Type"] = contentType
            });
    }

    public PreSignedUrlResult GeneratePresignedDownloadUrl(
        string path,
        string? downloadFileName = null,
        int expirationMinutes = 60)
    {
        var (containerName, blobPath) = ParsePath(path);
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        if (!string.IsNullOrEmpty(downloadFileName))
        {
            sasBuilder.ContentDisposition = $"attachment; filename=\"{SanitizeFileName(downloadFileName)}\"";
        }

        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        return new PreSignedUrlResult(
            Success: true,
            Url: sasUri.ToString(),
            ExpiresAt: DateTimeOffset.UtcNow.AddMinutes(expirationMinutes));
    }

    #endregion

    #region Azure Defender / Scan Tag Operations

    public async Task<Dictionary<string, string>?> GetScanTagsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobPath) = ParsePath(path);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var tagsResponse = await blobClient.GetTagsAsync(cancellationToken: cancellationToken);

            return tagsResponse.Value.Tags.ToDictionary(t => t.Key, t => t.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Blob not found when getting tags: {Path}", path);
            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to get blob tags: {Path}", path);
            return null;
        }
    }

    public async Task<bool> SetScanTagsAsync(
        string path,
        string scanResult,
        string? scanEngine = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (containerName, blobPath) = ParsePath(path);
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            var tags = new Dictionary<string, string>
            {
                ["scan-result"] = scanResult,
                ["scan-timestamp"] = DateTimeOffset.UtcNow.ToString("O")
            };

            if (!string.IsNullOrEmpty(scanEngine))
            {
                tags["scan-engine"] = scanEngine;
            }

            await blobClient.SetTagsAsync(tags, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Set scan tags on blob {Path}: {Result}",
                path, scanResult);

            return true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to set scan tags: {Path}", path);
            return false;
        }
    }

    #endregion

    #region Block Blob (Multipart) Upload

    public async Task<MultipartUploadInfo> InitiateMultipartUploadAsync(
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var blobPath = GetStoragePath(folder, fileName);
        var containerName = GetContainerName(folder);
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var uploadId = Guid.NewGuid().ToString("N");

        _logger.LogInformation(
            "Initiated multipart upload for {BlobPath}, session: {UploadId}",
            blobPath, uploadId);

        return new MultipartUploadInfo(uploadId, blobPath, containerName);
    }

    public string GeneratePartUploadUrl(
        string containerName,
        string blobPath,
        string uploadId,
        int partNumber,
        int expirationMinutes = 120)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blockBlobClient = containerClient.GetBlockBlobClient(blobPath);

        var blockId = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{uploadId}-{partNumber:D5}"));

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobPath,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Write);

        var sasUri = blockBlobClient.GenerateSasUri(sasBuilder);

        var uriBuilder = new UriBuilder(sasUri);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
        query["comp"] = "block";
        query["blockid"] = blockId;
        uriBuilder.Query = query.ToString();

        return uriBuilder.ToString();
    }

    public async Task<string> CompleteMultipartUploadAsync(
        string containerName,
        string blobPath,
        string uploadId,
        List<string> blockIds,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blockBlobClient = containerClient.GetBlockBlobClient(blobPath);

        await blockBlobClient.CommitBlockListAsync(
            blockIds,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Completed multipart upload for {BlobPath}, {BlockCount} blocks",
            blobPath, blockIds.Count);

        return blockBlobClient.Uri.ToString();
    }

    public async Task AbortMultipartUploadAsync(
        string containerName,
        string blobPath,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

        _logger.LogInformation("Aborted multipart upload for {BlobPath}", blobPath);
    }

    #endregion

    #region Helper Methods

    private string GetContainerName(string folder)
    {
        return folder == _options.TempFolder
            ? _options.Azure.TempContainerName ?? "auction-temp"
            : _options.Azure.MediaContainerName ?? "auction-storage";
    }

    private (string container, string blobPath) ParsePath(string path)
    {

        var parts = path.Split('/', 2);
        if (parts.Length == 2 &&
            !path.StartsWith(_options.TempFolder) &&
            !path.StartsWith(_options.PermanentFolder))
        {
            return (parts[0], parts[1]);
        }

        var container = path.StartsWith(_options.TempFolder)
            ? _options.Azure.TempContainerName ?? "auction-temp"
            : _options.Azure.MediaContainerName ?? "auction-storage";

        return (container, path);
    }

    private static string SanitizeFileName(string fileName)
    {
        return fileName
            .Replace("\"", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace(";", "_");
    }

    private static async Task<string> ComputeStreamChecksumAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToBase64String(hash);
    }

    #endregion
}
