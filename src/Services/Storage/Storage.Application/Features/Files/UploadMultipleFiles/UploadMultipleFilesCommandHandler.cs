using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using MassTransit;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using StorageService.Contracts.Events;

namespace Storage.Application.Features.Files.UploadMultipleFiles;

public class UploadMultipleFilesCommandHandler(
    IFileStorageService fileStorageService,
    IStoredFileRepository repository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    ILogger<UploadMultipleFilesCommandHandler> logger)
    : ICommandHandler<UploadMultipleFilesCommand, BatchUploadResultDto>
{
    private const int MaxConcurrentUploads = 5;

    public async Task<Result<BatchUploadResultDto>> Handle(
        UploadMultipleFilesCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Uploading {Count} files with max {Concurrency} concurrent",
            request.Files.Count, MaxConcurrentUploads);

        var storedFiles = new StoredFile[request.Files.Count];
        using var semaphore = new SemaphoreSlim(MaxConcurrentUploads);

        var uploadTasks = request.Files.Select((file, index) =>
            UploadSingleFileAsync(semaphore, file, request, index, storedFiles, cancellationToken));

        await Task.WhenAll(uploadTasks);

        await repository.AddRangeAsync(storedFiles.ToList(), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var publishTasks = storedFiles.Select(storedFile =>
            publishEndpoint.Publish(new FileUploadedEvent
            {
                FileId = storedFile.Id,
                FileName = storedFile.FileName,
                ContentType = storedFile.ContentType,
                FileSize = storedFile.FileSize,
                Url = storedFile.Url,
                OwnerId = storedFile.OwnerId,
                UploadedAt = storedFile.CreatedAt
            }, cancellationToken));

        await Task.WhenAll(publishTasks);

        logger.LogInformation("Uploaded {Count} files", storedFiles.Length);

        var dtos = storedFiles.Select(MapToDto).ToList();
        return Result.Success(new BatchUploadResultDto(dtos));
    }

    private async Task UploadSingleFileAsync(
        SemaphoreSlim semaphore,
        UploadFileItem file,
        UploadMultipleFilesCommand request,
        int index,
        StoredFile[] results,
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
                StorageProvider.Local);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static StoredFileDto MapToDto(StoredFile file) =>
        new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt);
}
