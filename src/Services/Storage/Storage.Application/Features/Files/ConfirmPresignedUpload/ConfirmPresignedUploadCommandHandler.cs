using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.Constants;
using Microsoft.Extensions.Options;
using Storage.Application.DTOs.Audit;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Constants;
using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Application.Features.Files.ConfirmPresignedUpload;

public class ConfirmPresignedUploadCommandHandler(
    IFileStorageService fileStorageService,
    IStoredFileRepository repository,
    IUnitOfWork unitOfWork,
    IOptions<FileStorageSettings> storageSettings,
    ILogger<ConfirmPresignedUploadCommandHandler> logger,
    IAuditPublisher auditPublisher)
    : ICommandHandler<ConfirmPresignedUploadCommand, StoredFileDto>
{
    public async Task<Result<StoredFileDto>> Handle(
        ConfirmPresignedUploadCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Confirming presigned upload for: {StoredFileName}", request.StoredFileName);

        var exists = await fileStorageService.ExistsAsync(request.StoredFileName, cancellationToken);

        if (!exists)
        {
            return Result.Failure<StoredFileDto>(StorageErrors.FileNotFoundInStorage);
        }

        var url = await fileStorageService.GetUrlAsync(request.StoredFileName, cancellationToken);
        var provider = StorageDefaults.Providers.Resolve(storageSettings.Value.Provider);

        var storedFile = StoredFile.Create(
            request.FileName,
            request.StoredFileName,
            request.ContentType,
            request.FileSize,
            url ?? string.Empty,
            request.SubFolder,
            request.OwnerId,
            provider);

        await repository.AddAsync(storedFile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditPublisher.PublishAsync(
            storedFile.Id,
            StoredFileAuditData.FromStoredFile(storedFile),
            AuditAction.Created,
            metadata: new Dictionary<string, object>
            {
                [AuditMetadataKeys.FileName] = storedFile.FileName,
                [AuditMetadataKeys.FileSize] = storedFile.FileSize,
                [AuditMetadataKeys.Provider] = provider.ToString(),
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
