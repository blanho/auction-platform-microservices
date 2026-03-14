using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using Storage.Application.DTOs;
using Storage.Application.Errors;

namespace Storage.Application.Features.Files.GeneratePresignedUpload;

public class GeneratePresignedUploadCommandHandler(
    IFileStorageService fileStorageService,
    ILogger<GeneratePresignedUploadCommandHandler> logger)
    : ICommandHandler<GeneratePresignedUploadCommand, PresignedUploadDto>
{
    public async Task<Result<PresignedUploadDto>> Handle(
        GeneratePresignedUploadCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Generating presigned upload URL for: {FileName}", request.FileName);

        var presignedRequest = new PresignedUploadRequest(
            FileName: request.FileName,
            ContentType: request.ContentType,
            FileSize: request.FileSize,
            SubFolder: request.SubFolder,
            OwnerId: request.OwnerId
        );

        var result = await fileStorageService.GenerateUploadSasTokenAsync(presignedRequest, cancellationToken);

        if (result is null)
        {
            return Result.Failure<PresignedUploadDto>(StorageErrors.PresignedUrlNotSupported);
        }

        return Result.Success(new PresignedUploadDto(
            FileId: result.FileId,
            StoredFileName: result.StoredFileName,
            UploadUrl: result.UploadUrl,
            Headers: result.Headers,
            ExpiresAt: result.ExpiresAt
        ));
    }
}
