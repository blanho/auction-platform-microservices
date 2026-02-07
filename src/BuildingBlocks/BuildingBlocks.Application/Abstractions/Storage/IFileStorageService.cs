#nullable enable
namespace BuildingBlocks.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);
    Task<FileDownloadResult?> DownloadAsync(string fileId, CancellationToken cancellationToken = default);
    Task<string?> GetUrlAsync(string fileId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string fileId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string fileId, CancellationToken cancellationToken = default);
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
