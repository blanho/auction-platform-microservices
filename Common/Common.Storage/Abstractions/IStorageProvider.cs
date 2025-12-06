using Common.Storage.Models;

namespace Common.Storage.Abstractions;

/// <summary>
/// Interface for storage providers (Local, Azure Blob, S3, etc.)
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// Upload a file to storage
    /// </summary>
    Task<FileUploadResult> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Download a file from storage
    /// </summary>
    Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a file from storage
    /// </summary>
    Task<bool> DeleteAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Move a file from one location to another
    /// </summary>
    Task<string?> MoveAsync(string sourcePath, string destinationFolder, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a file exists
    /// </summary>
    Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the URL for a file (if available)
    /// </summary>
    string? GetUrl(string path);
}

/// <summary>
/// High-level interface for file storage service with temp/permanent workflow
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file to temporary storage
    /// </summary>
    Task<FileUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirm and move a temporary file to permanent storage
    /// </summary>
    Task<FileUploadResult> ConfirmFileAsync(
        FileConfirmRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Download a file by ID
    /// </summary>
    Task<(Stream? Stream, FileMetadata? Metadata)> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a file by ID
    /// </summary>
    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get file metadata by ID
    /// </summary>
    Task<FileMetadata?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get files by entity
    /// </summary>
    Task<IEnumerable<FileMetadata>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);
}
