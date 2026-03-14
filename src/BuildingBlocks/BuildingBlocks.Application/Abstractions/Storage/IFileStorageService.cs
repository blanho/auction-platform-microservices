#nullable enable
namespace BuildingBlocks.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    Task<FileDownloadResult?> DownloadAsync(string storedFileName, CancellationToken cancellationToken = default);
    Task<string?> GetUrlAsync(string storedFileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string storedFileName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string storedFileName, CancellationToken cancellationToken = default);
    Task<PresignedUploadResult?> GenerateUploadSasTokenAsync(PresignedUploadRequest request, CancellationToken cancellationToken = default);
    Task<PresignedDownloadResult?> GenerateDownloadSasTokenAsync(string storedFileName, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
}

public record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long FileSize,
    string? SubFolder = null,
    Guid? OwnerId = null,
    Dictionary<string, string>? Metadata = null
);

public record FileUploadResult(
    string FileId,
    string FileName,
    string StoredFileName,
    string ContentType,
    long FileSize,
    string Url,
    DateTimeOffset UploadedAt
);

public record FileDownloadResult(
    Stream Content,
    string FileName,
    string ContentType,
    long FileSize
);

public record PresignedUploadRequest(
    string FileName,
    string ContentType,
    long FileSize,
    string? SubFolder = null,
    Guid? OwnerId = null
);

public record PresignedUploadResult(
    string FileId,
    string StoredFileName,
    string UploadUrl,
    Dictionary<string, string> Headers,
    DateTimeOffset ExpiresAt
);

public record PresignedDownloadResult(
    string DownloadUrl,
    string FileName,
    string ContentType,
    DateTimeOffset ExpiresAt
);
