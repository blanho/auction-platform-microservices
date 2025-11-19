using Storage.Application.DTOs;
using Storage.Domain.Enums;

namespace Storage.Application.Interfaces;

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

    Task<DownloadUrlResponse?> GetDownloadUrlWithPermissionCheckAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<FileMetadataDto?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<IEnumerable<FileMetadataDto>> GetByOwnerAsync(
        string ownerService,
        string ownerId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<FileMetadataDto>> GetByResourceAsync(
        Guid resourceId,
        StorageResourceType resourceType,
        CancellationToken cancellationToken = default);

    Task<FileUploadResult> DirectUploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string ownerService,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);

    Task<ScanStatusResponse> CheckScanStatusAsync(
        Guid fileId,
        CancellationToken cancellationToken = default);

    Task<SubmitFileResponse> SubmitFileAsync(
        SubmitFileRequest request,
        CancellationToken cancellationToken = default);

    Task<MultipartUploadSession?> InitiateMultipartUploadAsync(
        InitiateMultipartUploadRequest request,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default);

    Task<FileUploadResult> CompleteMultipartUploadAsync(
        CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken = default);
}
