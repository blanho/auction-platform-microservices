using BuildingBlocks.Application.CQRS.Commands;
using Storage.Application.DTOs;

namespace Storage.Application.Features.Files.GeneratePresignedUpload;

public record GeneratePresignedUploadCommand(
    string FileName,
    string ContentType,
    long FileSize,
    string? SubFolder = null,
    Guid? OwnerId = null
) : ICommand<PresignedUploadDto>;
