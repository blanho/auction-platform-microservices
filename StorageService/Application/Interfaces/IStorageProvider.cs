namespace StorageService.Application.Interfaces;

public interface IStorageProvider
{
    Task<StorageUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);
    
    Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
    
    Task<string?> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
    
    Task<PreSignedUrlResult> GenerateUploadUrlAsync(
        string fileName,
        string contentType,
        string folder,
        int expirationMinutes = 15,
        CancellationToken cancellationToken = default);
    
    Task<PreSignedUrlResult> GenerateDownloadUrlAsync(
        string path,
        int expirationMinutes = 15,
        CancellationToken cancellationToken = default);
    
    Task<string?> ComputeChecksumAsync(string path, CancellationToken cancellationToken = default);
    
    string GetStoragePath(string folder, string fileName);
}

public record StorageUploadResult(
    bool Success, 
    string? Path, 
    string? Url, 
    string? Checksum = null,
    string? Error = null);

public record PreSignedUrlResult(
    bool Success,
    string? Url,
    DateTimeOffset? ExpiresAt,
    Dictionary<string, string>? RequiredHeaders = null,
    string? Error = null);
