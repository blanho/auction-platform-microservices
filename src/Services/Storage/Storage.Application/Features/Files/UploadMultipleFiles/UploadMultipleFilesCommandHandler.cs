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
    public async Task<Result<BatchUploadResultDto>> Handle(
        UploadMultipleFilesCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Uploading {Count} files", request.Files.Count);

        var storedFiles = new List<StoredFile>();

        foreach (var file in request.Files)
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

            var storedFile = StoredFile.Create(
                uploadResult.FileName,
                uploadResult.StoredFileName,
                uploadResult.ContentType,
                uploadResult.FileSize,
                uploadResult.Url,
                request.SubFolder,
                request.OwnerId,
                StorageProvider.Local);

            storedFiles.Add(storedFile);
        }

        await repository.AddRangeAsync(storedFiles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var storedFile in storedFiles)
        {
            await publishEndpoint.Publish(new FileUploadedEvent
            {
                FileId = storedFile.Id,
                FileName = storedFile.FileName,
                ContentType = storedFile.ContentType,
                FileSize = storedFile.FileSize,
                Url = storedFile.Url,
                OwnerId = storedFile.OwnerId,
                UploadedAt = storedFile.CreatedAt
            }, cancellationToken);
        }

        logger.LogInformation("Uploaded {Count} files", storedFiles.Count);

        var dtos = storedFiles.Select(MapToDto).ToList();
        return Result.Success(new BatchUploadResultDto(dtos));
    }

    private static StoredFileDto MapToDto(StoredFile file) =>
        new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt);
}
