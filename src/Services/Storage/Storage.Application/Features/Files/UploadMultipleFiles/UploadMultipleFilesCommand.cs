using BuildingBlocks.Application.CQRS.Commands;

namespace Storage.Application.Features.Files.UploadMultipleFiles;

public record UploadMultipleFilesCommand(
    List<UploadFileItem> Files,
    string? SubFolder = null,
    Guid? OwnerId = null
) : ICommand<BatchUploadResultDto>;

public record UploadFileItem(
    Stream Content,
    string FileName,
    string ContentType,
    long FileSize,
    Dictionary<string, string>? Metadata = null
);
