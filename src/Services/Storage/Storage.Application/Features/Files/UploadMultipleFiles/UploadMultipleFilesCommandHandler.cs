using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using Microsoft.Extensions.Options;
using Storage.Application.DTOs.Audit;
using Storage.Application.Interfaces;
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

        var provider = ResolveStorageProvider(storageSettings.Value.Provider);
        var storedFiles = new StoredFile?[request.Files.Count];
        var errors = new List<string>();
        using var semaphore = new SemaphoreSlim(MaxConcurrentUploads);

        var uploadTasks = request.Files.Select((file, index) =>
            UploadSingleFileAsync(semaphore, file, request, index, storedFiles, errors, provider, cancellationToken));

        await Task.WhenAll(uploadTasks);

        var successfulFiles = storedFiles.Where(f => f is not null).Cast<StoredFile>().ToList();

        if (successfulFiles.Count == 0)
        {
            return Result.Failure<BatchUploadResultDto>(
                Error.Create("Storage.BatchUploadFailed", "All file uploads failed"));
        }

        await repository.AddRangeAsync(successfulFiles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var storedFile in successfulFiles)
        {
            await auditPublisher.PublishAsync(
                storedFile.Id,
                StoredFileAuditData.FromStoredFile(storedFile),
                AuditAction.Created,
                metadata: new Dictionary<string, object>
                {
                    ["FileName"] = storedFile.FileName,
                    ["FileSize"] = storedFile.FileSize,
                    ["BatchUpload"] = true
                },
                cancellationToken: cancellationToken);
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
        SemaphoreSlim semaphore,
        UploadFileItem file,
        UploadMultipleFilesCommand request,
        int index,
        StoredFile?[] results,
        List<string> errors,
        StorageProvider provider,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
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

            results[index] = StoredFile.Create(
                uploadResult.FileName,
                uploadResult.StoredFileName,
                uploadResult.ContentType,
                uploadResult.FileSize,
                uploadResult.Url,
                request.SubFolder,
                request.OwnerId,
                provider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload file {FileName} at index {Index}", file.FileName, index);
            lock (errors)
            {
                errors.Add($"{file.FileName}: {ex.Message}");
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static StorageProvider ResolveStorageProvider(string providerName) =>
        string.Equals(providerName, "AzureBlob", StringComparison.OrdinalIgnoreCase)
            ? StorageProvider.AzureBlob
            : StorageProvider.Local;

    private static StoredFileDto MapToDto(StoredFile file) =>
        new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt);
}
