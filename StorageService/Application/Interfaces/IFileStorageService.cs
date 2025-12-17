using StorageService.Application.DTOs;

namespace StorageService.Application.Interfaces;

public interface IFileStorageService
{
    Task<UploadUrlResponse> RequestUploadAsync(
        RequestUploadDto request,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);
    
    Task<FileUploadResult> ConfirmUploadAsync(
        ConfirmUploadRequest request,
        CancellationToken cancellationToken = default);
    
    Task<FileUploadResult> ConfirmFileAsync(
        FileConfirmRequest request,
        CancellationToken cancellationToken = default);
    
    Task<DownloadUrlResponse?> GetDownloadUrlAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    Task<FileMetadataDto?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<FileMetadataDto>> GetByOwnerAsync(
        string ownerService,
        string ownerId,
        CancellationToken cancellationToken = default);
    
    Task<FileUploadResult> DirectUploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string ownerService,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);
}
