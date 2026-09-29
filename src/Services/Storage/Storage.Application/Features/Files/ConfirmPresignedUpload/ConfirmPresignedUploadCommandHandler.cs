using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.Constants;
using Storage.Application.DTOs.Audit;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Constants;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using StorageService.Contracts.Reports;

namespace Storage.Application.Features.Files.ConfirmPresignedUpload;

public class ConfirmPresignedUploadCommandHandler(
    IFileStorageService fileStorageService,
    IStoredFileRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<ConfirmPresignedUploadCommandHandler> logger,
    IAuditPublisher auditPublisher)
    : ICommandHandler<ConfirmPresignedUploadCommand, StoredFileDto>
{
    public async Task<Result<StoredFileDto>> Handle(
        ConfirmPresignedUploadCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Confirming presigned upload for: {StoredFileName}", request.StoredFileName);

        if (ReportStorageContract.IsPrivatePath(request.StoredFileName))
        {
            return Result.Failure<StoredFileDto>(StorageErrors.FileNotFoundInStorage);
        }

        var storedFile = await repository.GetByStoredFileNameAsync(request.StoredFileName, cancellationToken);
        if (storedFile is null || request.OwnerId is null || storedFile.OwnerId != request.OwnerId ||
            storedFile.FileName != request.FileName || storedFile.ContentType != request.ContentType ||
            storedFile.FileSize != request.FileSize || storedFile.SubFolder != request.SubFolder ||
            ReportStorageContract.IsPrivatePath(storedFile.SubFolder))
            return Result.Failure<StoredFileDto>(StorageErrors.FileNotFoundInStorage);

        if (storedFile.Status == FileStatus.Ready)
            return Result.Success(new StoredFileDto(storedFile.Id, storedFile.FileName,
                storedFile.ContentType, storedFile.FileSize, storedFile.Url, storedFile.CreatedAt));
        if (storedFile.Status != FileStatus.Pending || !storedFile.UploadExpiresAt.HasValue || storedFile.UploadExpiresAt <= DateTimeOffset.UtcNow)
            return Result.Failure<StoredFileDto>(StorageErrors.FileNotFoundInStorage);

        var exists = await fileStorageService.ExistsAsync(request.StoredFileName, cancellationToken);

        if (!exists)
        {
            return Result.Failure<StoredFileDto>(StorageErrors.FileNotFoundInStorage);
        }

        var url = await fileStorageService.GetUrlAsync(request.StoredFileName, cancellationToken);
        storedFile.ConfirmUpload(url ?? string.Empty);
        repository.Update(storedFile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditPublisher.PublishAsync(
            storedFile.Id,
            StoredFileAuditData.FromStoredFile(storedFile),
            AuditAction.Created,
            metadata: new Dictionary<string, object>
            {
                [AuditMetadataKeys.FileName] = storedFile.FileName,
                [AuditMetadataKeys.FileSize] = storedFile.FileSize,
                [AuditMetadataKeys.Provider] = storedFile.Provider.ToString(),
                [AuditMetadataKeys.PresignedUpload] = true
            },
            cancellationToken: cancellationToken);

        logger.LogInformation("Presigned upload confirmed: {FileId} ({FileName})",
            storedFile.Id, storedFile.FileName);

        return Result.Success(new StoredFileDto(
            storedFile.Id, storedFile.FileName, storedFile.ContentType,
            storedFile.FileSize, storedFile.Url, storedFile.CreatedAt));
    }
}
