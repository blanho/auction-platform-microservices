using BuildingBlocks.Application.CQRS.Commands;

namespace Storage.Application.Features.Files.DeleteFile;

public record DeleteFileCommand(Guid FileId, Guid? RequestedById = null) : ICommand;
