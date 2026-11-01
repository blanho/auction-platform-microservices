using Storage.Application.Mappings;
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

        return Result.Success(file.ToDetailsDto());
    }

}
