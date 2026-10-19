using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.Constants;
using Microsoft.Extensions.Options;
using Storage.Application.DTOs;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Constants;
using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Application.Features.Files.UploadMultipleFiles;

public class UploadMultipleFilesCommandHandler(
    IFileStorageService fileStorageService,
    IStoredFileRepository repository,
    IUnitOfWork unitOfWork,
    IOptions<FileStorageSettings> storageSettings,
    ILogger<UploadMultipleFilesCommandHandler> logger,
    IAuditPublisher auditPublisher)
    : ICommandHandler<UploadMultipleFilesCommand, BatchUploadResultDto>
{
    private const int MaxConcurrentUploads = 5;

    public async Task<Result<BatchUploadResultDto>> Handle(
        UploadMultipleFilesCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Uploading {Count} files with max {Concurrency} concurrent",
            request.Files.Count, MaxConcurrentUploads);

        var provider = StorageDefaults.Providers.Resolve(storageSettings.Value.Provider);
        var storedFiles = new StoredFile?[request.Files.Count];
        var errors = new List<string>();
        using var semaphore = new SemaphoreSlim(MaxConcurrentUploads);
        var batch = new UploadBatchContext(semaphore, storedFiles, errors, provider);

        var uploadTasks = request.Files.Select((file, index) =>
            UploadSingleFileAsync(file, request, index, batch, cancellationToken));

        await Task.WhenAll(uploadTasks);

        var successfulFiles = storedFiles.Where(f => f is not null).Cast<StoredFile>().ToList();

        if (successfulFiles.Count == 0)
        {
            return Result.Failure<BatchUploadResultDto>(StorageErrors.BatchUploadFailed);
        }

        try
        {
            await repository.AddRangeAsync(successfulFiles, cancellationToken);
            foreach (var storedFile in successfulFiles)
            {
                await auditPublisher.PublishAsync(
                    storedFile.Id,
                    StoredFileAuditData.FromStoredFile(storedFile),
                    AuditAction.Created,
                    metadata: new Dictionary<string, object>
                    {
                        [AuditMetadataKeys.FileName] = storedFile.FileName,
                        [AuditMetadataKeys.FileSize] = storedFile.FileSize,
                        [AuditMetadataKeys.BatchUpload] = true
                    },
                    cancellationToken: cancellationToken);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await RollbackUploadedBlobsAsync(successfulFiles);

            throw;
        }

        if (errors.Count > 0)
        {
            logger.LogWarning("Batch upload completed with {ErrorCount} failures: {Errors}",
                errors.Count, string.Join("; ", errors));
        }

        logger.LogInformation("Uploaded {SuccessCount}/{TotalCount} files",
            successfulFiles.Count, request.Files.Count);

        var dtos = successfulFiles.Select(MapToDto).ToList();
        return Result.Success(new BatchUploadResultDto(dtos));
    }

    private async Task UploadSingleFileAsync(
        UploadFileItem file,
        UploadMultipleFilesCommand request,
        int index,
        UploadBatchContext batch,
        CancellationToken cancellationToken)
    {
        await batch.Semaphore.WaitAsync(cancellationToken);
        try
        {
            var uploadRequest = new FileUploadRequest(
                file.Content,
                file.FileName,
                file.ContentType,
                file.FileSize,
                request.SubFolder,
                request.OwnerId,
                file.Metadata);

            var uploadResult = await fileStorageService.UploadAsync(uploadRequest, cancellationToken);

            batch.Results[index] = StoredFile.Create(new CreateStoredFileParams
            {
                FileName = uploadResult.FileName,
                StoredFileName = uploadResult.StoredFileName,
                ContentType = uploadResult.ContentType,
                FileSize = uploadResult.FileSize,
                Url = uploadResult.Url,
                SubFolder = request.SubFolder,
                OwnerId = request.OwnerId,
                Provider = batch.Provider
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload file {FileName} at index {Index}", file.FileName, index);
            lock (batch.Errors)
            {
                batch.Errors.Add($"{file.FileName}: {ex.Message}");
            }
        }
        finally
        {
            batch.Semaphore.Release();
        }
    }

    private async Task RollbackUploadedBlobsAsync(
        IEnumerable<StoredFile> uploadedFiles)
    {
        foreach (var storedFileName in uploadedFiles.Select(file => file.StoredFileName))
        {
            try
            {
                if (!await fileStorageService.DeleteAsync(storedFileName, CancellationToken.None))
                {
                    logger.LogWarning(
                        "Failed to roll back blob '{StoredFileName}' after DB commit failure. Manual cleanup required.",
                        storedFileName);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to roll back blob '{StoredFileName}' after DB commit failure. Manual cleanup required.",
                    storedFileName);
            }
        }
    }

    private sealed record UploadBatchContext(SemaphoreSlim Semaphore, StoredFile?[] Results,
        List<string> Errors, StorageProvider Provider);

    private static StoredFileDto MapToDto(StoredFile file) =>
        new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt);
}
