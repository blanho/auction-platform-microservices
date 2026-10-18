using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Microsoft.Extensions.Options;
using StorageService.Contracts.Reports;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Application.CQRS.Commands;
using Storage.Application.DTOs;
using Storage.Application.Errors;

namespace Storage.Application.Features.Files.GeneratePresignedUpload;

public class GeneratePresignedUploadCommandHandler(
    IFileStorageService fileStorageService,
    IStoredFileRepository repository,
    IUnitOfWork unitOfWork,
    IOptions<FileStorageSettings> settings,
    ILogger<GeneratePresignedUploadCommandHandler> logger)
    : ICommandHandler<GeneratePresignedUploadCommand, PresignedUploadDto>
{
    public async Task<Result<PresignedUploadDto>> Handle(
        GeneratePresignedUploadCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Generating presigned upload URL for: {FileName}", request.FileName);

        if (request.OwnerId is null || request.OwnerId == Guid.Empty ||
            ReportStorageContract.IsPrivatePath(request.SubFolder))
            return Result.Failure<PresignedUploadDto>(StorageErrors.FileNotFoundInStorage);

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

        var reservation = StoredFile.ReserveUpload(new ReserveStoredFileUploadParams
        {
            Id = Guid.Parse(result.FileId),
            FileName = request.FileName,
            StoredFileName = result.StoredFileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            SubFolder = request.SubFolder,
            OwnerId = request.OwnerId.Value,
            Provider = StorageDefaults.Providers.Resolve(settings.Value.Provider),
            ExpiresAt = result.ExpiresAt
        });
        await repository.AddAsync(reservation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new PresignedUploadDto(
            FileId: result.FileId,
            StoredFileName: result.StoredFileName,
            UploadUrl: result.UploadUrl,
            Headers: result.Headers,
            ExpiresAt: result.ExpiresAt
        ));
    }
}
