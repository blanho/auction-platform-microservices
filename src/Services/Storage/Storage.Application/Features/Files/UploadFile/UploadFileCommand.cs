using BuildingBlocks.Application.CQRS.Commands;

namespace Storage.Application.Features.Files.UploadFile;

public record UploadFileCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long FileSize,
    string? SubFolder = null,
    Guid? OwnerId = null,
    Dictionary<string, string>? Metadata = null
) : ICommand<StoredFileDto>;
