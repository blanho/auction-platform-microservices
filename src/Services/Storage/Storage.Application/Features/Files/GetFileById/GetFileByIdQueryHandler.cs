using BuildingBlocks.Application.CQRS.Queries;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;

namespace Storage.Application.Features.Files.GetFileById;

public class GetFileByIdQueryHandler(
    IStoredFileRepository repository,
    ILogger<GetFileByIdQueryHandler> logger)
    : IQueryHandler<GetFileByIdQuery, StoredFileDto>
{
    public async Task<Result<StoredFileDto>> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting file by ID: {FileId}", request.FileId);

        var file = await repository.GetByIdAsync(request.FileId, cancellationToken);

        if (file is null)
        {
            return Result.Failure<StoredFileDto>(StorageErrors.FileNotFound(request.FileId));
        }

        return Result.Success(MapToDto(file));
    }

    private static StoredFileDto MapToDto(StoredFile file) =>
        new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt);
}
