using BuildingBlocks.Application.CQRS.Queries;

namespace Storage.Application.Features.Files.GetFileById;

public record GetFileByIdQuery(Guid FileId) : IQuery<StoredFileDto>;
