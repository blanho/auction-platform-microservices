using BuildingBlocks.Application.CQRS.Queries;

namespace Storage.Application.Features.Files.GetFileUrl;

public record GetFileUrlQuery(Guid FileId) : IQuery<FileUrlDto>;
