using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using MassTransit;
using Storage.Application.DTOs.Audit;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using StorageService.Contracts.Events;

namespace Storage.Application.Features.Files.DeleteFile;

public class DeleteFileCommandHandler(
    IStoredFileRepository repository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    ILogger<DeleteFileCommandHandler> logger,
    IAuditPublisher auditPublisher)
    : ICommandHandler<DeleteFileCommand>
{
    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Deleting file: {FileId}", request.FileId);

        var file = await repository.GetByIdAsync(request.FileId, cancellationToken);

        if (file is null)
        {
            return Result.Failure(StorageErrors.FileNotFound(request.FileId));
        }

        var oldFileData = StoredFileAuditData.FromStoredFile(file);

        var deleted = await fileStorageService.DeleteAsync(file.StoredFileName, cancellationToken);

        if (!deleted)
        {
            logger.LogWarning("Failed to delete physical file {StoredFileName} for FileId {FileId}",
                file.StoredFileName, file.Id);
        }

        file.MarkAsDeleted(null);
        repository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditPublisher.PublishAsync(
            file.Id,
            StoredFileAuditData.FromStoredFile(file),
            AuditAction.Deleted,
            oldFileData,
            cancellationToken: cancellationToken);

        await publishEndpoint.Publish(new FileDeletedEvent
        {
            FileId = file.Id,
            FileName = file.FileName,
            OwnerId = file.OwnerId,
            DeletedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        logger.LogInformation("File deleted: {FileId} ({FileName})", file.Id, file.FileName);

        return Result.Success();
    }
}
