namespace StorageService.Application.Interfaces;

public interface IStorageProvider
{
    Task<StorageUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);
    
    Task<Stream> DownloadAsync(string path, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
    
    Task<string> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
    
    string GetUrl(string path);
}

public record StorageUploadResult(bool Success, string Path, string Url, string Error = null);
