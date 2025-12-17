using StorageService.Application.DTOs;

namespace StorageService.Application.Interfaces;

public interface IFileStorageService
{
    Task<FileUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string uploadedBy = null,
        CancellationToken cancellationToken = default);
    
    Task<FileUploadResult> ConfirmFileAsync(
        FileConfirmRequest request,
        CancellationToken cancellationToken = default);
    
    Task<(Stream Stream, FileMetadataDto Metadata)> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    Task<FileMetadataDto> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<FileMetadataDto>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);
}
