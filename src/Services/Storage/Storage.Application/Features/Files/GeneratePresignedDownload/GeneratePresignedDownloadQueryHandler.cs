using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Queries;
using Storage.Application.DTOs;
using Storage.Application.Errors;
using Storage.Application.Interfaces;

namespace Storage.Application.Features.Files.GeneratePresignedDownload;

public class GeneratePresignedDownloadQueryHandler(
    IStoredFileRepository repository,
    IFileStorageService fileStorageService,
    ILogger<GeneratePresignedDownloadQueryHandler> logger)
    : IQueryHandler<GeneratePresignedDownloadQuery, PresignedDownloadDto>
{
    public async Task<Result<PresignedDownloadDto>> Handle(
        GeneratePresignedDownloadQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Generating presigned download URL for file: {FileId}", request.FileId);

        var file = await repository.GetByIdAsync(request.FileId, cancellationToken);

        if (file is null)
        {
            return Result.Failure<PresignedDownloadDto>(StorageErrors.FileNotFound(request.FileId));
        }

        var result = await fileStorageService.GenerateDownloadSasTokenAsync(
            file.StoredFileName,
            expiry: TimeSpan.FromHours(1),
            cancellationToken);

        if (result is null)
        {
            return Result.Failure<PresignedDownloadDto>(StorageErrors.PresignedUrlNotSupported);
        }

        return Result.Success(new PresignedDownloadDto(
            DownloadUrl: result.DownloadUrl,
            FileName: result.FileName,
            ContentType: result.ContentType,
            ExpiresAt: result.ExpiresAt
        ));
    }
}
