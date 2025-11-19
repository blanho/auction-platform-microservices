using Storage.Application.DTOs;
using Storage.Domain.Constants;

namespace Storage.Application.Interfaces;

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
        int expirationMinutes = StorageDefaults.DefaultUrlExpirationMinutes,
        CancellationToken cancellationToken = default);
    
    Task<PreSignedUrlResult> GenerateDownloadUrlAsync(
        string path,
        int expirationMinutes = StorageDefaults.DefaultUrlExpirationMinutes,
        CancellationToken cancellationToken = default);
    
    Task<string?> ComputeChecksumAsync(string path, CancellationToken cancellationToken = default);
    
    string GetStoragePath(string folder, string fileName);
}
