using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Queries;
using Storage.Application.Errors;
using Storage.Application.Interfaces;

namespace Storage.Application.Features.Files.GetFileUrl;

public class GetFileUrlQueryHandler(
    IStoredFileRepository repository,
    IFileStorageService fileStorageService,
    ILogger<GetFileUrlQueryHandler> logger)
    : IQueryHandler<GetFileUrlQuery, FileUrlDto>
{
    public async Task<Result<FileUrlDto>> Handle(GetFileUrlQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting URL for file: {FileId}", request.FileId);

        var file = await repository.GetByIdAsync(request.FileId, cancellationToken);

        if (file is null)
        {
            return Result.Failure<FileUrlDto>(StorageErrors.FileNotFound(request.FileId));
        }

        var url = await fileStorageService.GetUrlAsync(file.StoredFileName, cancellationToken);

        if (url is null)
        {
            return Result.Failure<FileUrlDto>(StorageErrors.FileNotFound(request.FileId));
        }

        return Result.Success(new FileUrlDto(file.Id, url));
    }
}
