using BuildingBlocks.Application.CQRS.Queries;
using Storage.Application.Errors;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using StorageService.Contracts.Reports;

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

        if (file is null || file.Status != Storage.Domain.Enums.FileStatus.Ready || ReportStorageContract.IsPrivatePath(file.SubFolder) ||
            ReportStorageContract.IsPrivatePath(file.StoredFileName))
        {
            return Result.Failure<StoredFileDto>(StorageErrors.FileNotFound(request.FileId));
        }

        return Result.Success(MapToDto(file));
    }

    private static StoredFileDto MapToDto(StoredFile file)
    {
        ProcessedImageDto? image = null;
        if (file.Metadata.TryGetValue(StorageDefaults.Images.WidthKey, out var widthText) &&
            file.Metadata.TryGetValue(StorageDefaults.Images.HeightKey, out var heightText) &&
            file.Metadata.TryGetValue(StorageDefaults.Images.ThumbnailKey, out var thumbnail) &&
            int.TryParse(widthText, out var width) && int.TryParse(heightText, out var height))
            image = new ProcessedImageDto(width, height, thumbnail);
        return new(file.Id, file.FileName, file.ContentType, file.FileSize,
            file.Url, file.CreatedAt, file.Checksum, image);
    }
}
