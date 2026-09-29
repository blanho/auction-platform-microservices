using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using Storage.Application.DTOs.Audit;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using StorageService.Contracts.Reports;

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

        if (file is null || ReportStorageContract.IsPrivatePath(file.SubFolder) ||
            ReportStorageContract.IsPrivatePath(file.StoredFileName))
        {
            return Result.Failure(StorageErrors.FileNotFound(request.FileId));
        }

        var oldFileData = StoredFileAuditData.FromStoredFile(file);
        var storedFileName = file.StoredFileName;

        file.MarkAsDeleted(request.RequestedById);
        repository.Update(file);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await fileStorageService.DeleteAsync(storedFileName, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Physical deletion deferred to cleanup for file {FileId}", file.Id);
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
