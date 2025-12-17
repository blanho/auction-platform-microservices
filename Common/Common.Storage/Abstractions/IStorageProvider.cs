using Common.Storage.Models;

namespace Common.Storage.Abstractions;

public interface IStorageProvider
{
    Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);
    
    Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
    
    Task<string?> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
    
    string? GetUrl(string path);
}

public interface IFileStorageService
{
    Task<FileUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);
    
    Task<FileUploadResult> ConfirmFileAsync(
        FileConfirmRequest request,
        CancellationToken cancellationToken = default);
    
    Task<(Stream? Stream, FileMetadata? Metadata)> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes all files associated with an entity from a specific owner service.
    /// Used for event-driven cleanup when entities are deleted.
    /// </summary>
    /// <param name="ownerService">The service that owns the files (e.g., "auction", "product", "category")</param>
    /// <param name="entityId">The ID of the entity whose files should be deleted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of files deleted</returns>
    Task<int> DeleteByOwnerEntityAsync(
        string ownerService,
        string entityId,
        CancellationToken cancellationToken = default);
    
    Task<FileMetadata?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<FileMetadata>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);
}
