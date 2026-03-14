using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using Storage.Application.DTOs.Audit;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;

namespace Storage.Application.Features.Files.DeleteFile;

public class DeleteFileCommandHandler(
    IStoredFileRepository repository,
    IFileStorageService fileStorageService,
    IUnitOfWork unitOfWork,
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
        var storedFileName = file.StoredFileName;

        file.MarkAsDeleted(null);
        repository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var deleted = await fileStorageService.DeleteAsync(storedFileName, cancellationToken);

        if (!deleted)
        {
            logger.LogWarning("Failed to delete physical file {StoredFileName} for FileId {FileId}",
                storedFileName, file.Id);
        }

        await auditPublisher.PublishAsync(
            file.Id,
            StoredFileAuditData.FromStoredFile(file),
            AuditAction.Deleted,
            oldFileData,
            cancellationToken: cancellationToken);

        logger.LogInformation("File deleted: {FileId} ({FileName})", file.Id, file.FileName);

        return Result.Success();
    }
}
