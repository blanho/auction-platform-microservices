using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.Constants;
using Microsoft.Extensions.Options;
using Storage.Application.DTOs.Audit;
using Storage.Application.Interfaces;
using Storage.Domain.Constants;
using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Application.Features.Files.UploadFile;

public class UploadFileCommandHandler(
    IFileStorageService fileStorageService,
    IStoredFileRepository repository,
    IUnitOfWork unitOfWork,
    IOptions<FileStorageSettings> storageSettings,
    ILogger<UploadFileCommandHandler> logger,
    IAuditPublisher auditPublisher)
    : ICommandHandler<UploadFileCommand, StoredFileDto>
{
    public async Task<Result<StoredFileDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Uploading file {FileName} ({ContentType}, {FileSize} bytes)",
            request.FileName, request.ContentType, request.FileSize);

        var uploadRequest = new FileUploadRequest(
            request.Content,
            request.FileName,
            request.ContentType,
            request.FileSize,
            request.SubFolder,
            request.OwnerId,
            request.Metadata);

        var uploadResult = await fileStorageService.UploadAsync(uploadRequest, cancellationToken);

        var provider = StorageDefaults.Providers.Resolve(storageSettings.Value.Provider);

        var storedFile = StoredFile.Create(
            uploadResult.FileName,
            uploadResult.StoredFileName,
            uploadResult.ContentType,
            uploadResult.FileSize,
            uploadResult.Url,
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
                [AuditMetadataKeys.Provider] = provider.ToString()
            },
            cancellationToken: cancellationToken);

        logger.LogInformation("File uploaded: {FileId} ({FileName})", storedFile.Id, storedFile.FileName);

        return Result.Success(MapToDto(storedFile));
    }

    private static StoredFileDto MapToDto(StoredFile file) =>
        new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt);
}
