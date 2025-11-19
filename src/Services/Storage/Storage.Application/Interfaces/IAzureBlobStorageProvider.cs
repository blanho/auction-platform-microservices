using Storage.Application.DTOs;

namespace Storage.Application.Interfaces;

public interface IAzureBlobStorageProvider : IStorageProvider
{

    PreSignedUrlResult GeneratePresignedUploadUrl(
        string fileName,
        string contentType,
        string folder,
        int expirationMinutes = 15);

    PreSignedUrlResult GeneratePresignedDownloadUrl(
        string path,
        string? downloadFileName = null,
        int expirationMinutes = 60);

    Task<Dictionary<string, string>?> GetScanTagsAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task<bool> SetScanTagsAsync(
        string path,
        string scanResult,
        string? scanEngine = null,
        CancellationToken cancellationToken = default);

    Task<MultipartUploadInfo> InitiateMultipartUploadAsync(
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    string GeneratePartUploadUrl(
        string containerName,
        string blobPath,
        string uploadId,
        int partNumber,
        int expirationMinutes = 120);

    Task<string> CompleteMultipartUploadAsync(
        string containerName,
        string blobPath,
        string uploadId,
        List<string> blockIds,
        CancellationToken cancellationToken = default);

    Task AbortMultipartUploadAsync(
        string containerName,
        string blobPath,
        CancellationToken cancellationToken = default);
}

public record MultipartUploadInfo(
    string UploadId,
    string Key,
    string Container);
