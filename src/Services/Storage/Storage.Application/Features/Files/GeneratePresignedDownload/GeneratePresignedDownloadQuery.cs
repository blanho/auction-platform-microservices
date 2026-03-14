using BuildingBlocks.Application.CQRS.Queries;
using Storage.Application.DTOs;

namespace Storage.Application.Features.Files.GeneratePresignedDownload;

public record GeneratePresignedDownloadQuery(Guid FileId) : IQuery<PresignedDownloadDto>;
