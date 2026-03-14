using BuildingBlocks.Application.CQRS.Commands;

namespace Storage.Application.Features.Files.ConfirmPresignedUpload;

public record ConfirmPresignedUploadCommand(
    string StoredFileName,
    string FileName,
    string ContentType,
    long FileSize,
    string? SubFolder,
    Guid? OwnerId
) : ICommand<StoredFileDto>;
