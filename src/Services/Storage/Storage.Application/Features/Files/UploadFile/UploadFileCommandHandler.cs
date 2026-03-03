using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using MassTransit;
using Microsoft.Extensions.Options;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using StorageService.Contracts.Events;

namespace Storage.Application.Features.Files.UploadFile;

public class UploadFileCommandHandler(
    IFileStorageService fileStorageService,
    IStoredFileRepository repository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IOptions<FileStorageSettings> storageSettings,
    ILogger<UploadFileCommandHandler> logger)
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

        var provider = ResolveStorageProvider(storageSettings.Value.Provider);

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("File uploaded: {FileId} ({FileName})", storedFile.Id, storedFile.FileName);

        return Result.Success(MapToDto(storedFile));
    }

    private static StorageProvider ResolveStorageProvider(string providerName) =>
        string.Equals(providerName, "AzureBlob", StringComparison.OrdinalIgnoreCase)
            ? StorageProvider.AzureBlob
            : StorageProvider.Local;

    private static StoredFileDto MapToDto(StoredFile file) =>
        new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt);
}
