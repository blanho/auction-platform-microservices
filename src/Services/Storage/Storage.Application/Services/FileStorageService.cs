using BuildingBlocks.Web.Exceptions;
using Storage.Contracts;
using MassTransit;
using Microsoft.Extensions.Options;
using Storage.Application.Configuration;
using Storage.Application.DTOs;
using Storage.Application.Extensions.Mappings;
using Storage.Application.Helpers;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using Storage.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Storage.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IStorageProvider _storageProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly StorageOptions _options;
    private readonly IVirusScanService? _virusScanService;
    private readonly IFilePermissionService? _permissionService;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IStorageProvider storageProvider,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IOptions<StorageOptions> options,
        ILogger<FileStorageService> logger,
        IVirusScanService? virusScanService = null,
        IFilePermissionService? permissionService = null)
    {
        _storageProvider = storageProvider;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _options = options.Value;
        _logger = logger;
        _virusScanService = virusScanService;
        _permissionService = permissionService;
    }

    public async Task<UploadUrlResponse> RequestUploadAsync(
        RequestUploadDto request,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        var validationError = FileValidationHelper.ValidateFile(request.FileName, request.ContentType, request.Size, _options);
        if (validationError != null)
        {
            throw new ValidationAppException(validationError, new Dictionary<string, string[]>());
        }

        var extension = Path.GetExtension(request.FileName)?.ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var storagePath = _storageProvider.GetStoragePath(_options.TempFolder, uniqueFileName);

        var storedFile = StoredFile.Create(
            fileName: uniqueFileName,
            originalFileName: request.FileName,
            contentType: request.ContentType,
            size: request.Size,
            provider: StoragePathHelper.GetStorageProvider(_options.DefaultProvider),
            storagePath: storagePath,
            ownerService: request.OwnerService,
            ownerId: null,
            uploadedBy: uploadedBy,
            bucketName: _options.Azure.TempContainerName,
            expiresIn: TimeSpan.FromHours(_options.TempFileExpirationHours),
            resourceType: request.ResourceType,
            portalType: request.PortalType
        );

        if (request.Metadata != null)
        {
            storedFile.SetMetadata(request.Metadata);
        }

        await _unitOfWork.StoredFiles.AddAsync(storedFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var urlResult = await _storageProvider.GenerateUploadUrlAsync(
            uniqueFileName,
            request.ContentType,
            _options.TempFolder,
            _options.UploadUrlExpirationMinutes,
            cancellationToken);

        if (!urlResult.Success)
        {
            storedFile.MarkFailed(urlResult.Error ?? "Failed to generate upload URL");
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ConflictException(urlResult.Error ?? "Failed to generate upload URL");
        }

        return new UploadUrlResponse
        {
            FileId = storedFile.Id,
            UploadUrl = urlResult.Url!,
            ExpiresAt = urlResult.ExpiresAt!.Value,
            RequiredHeaders = urlResult.RequiredHeaders,
            MaxSizeBytes = _options.MaxFileSize,
            RequiredContentType = request.ContentType
        };
    }

    public async Task<FileUploadResult> ConfirmUploadAsync(
        ConfirmUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(request.FileId, cancellationToken);

        if (storedFile == null)
        {
            return FileUploadResult.Fail("File not found");
        }

        if (storedFile.Status != FileStatus.Pending)
        {
            return FileUploadResult.Fail($"File is not pending upload. Current status: {storedFile.Status}");
        }

        var exists = await _storageProvider.ExistsAsync(storedFile.StoragePath, cancellationToken);
        if (!exists)
        {
            storedFile.MarkFailed("File not found in storage");
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return FileUploadResult.Fail("File was not uploaded to storage");
        }

        var actualChecksum = await _storageProvider.ComputeChecksumAsync(storedFile.StoragePath, cancellationToken);

        if (string.IsNullOrEmpty(actualChecksum))
        {
            storedFile.MarkFailed("Failed to compute file checksum");
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return FileUploadResult.Fail("Failed to compute file checksum");
        }

        if (request.Checksum != null && actualChecksum != request.Checksum)
        {
            storedFile.MarkFailed("Checksum mismatch");
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return FileUploadResult.Fail("Checksum verification failed");
        }

        storedFile.SetChecksum(actualChecksum);

        storedFile.MarkInTemp(storedFile.StoragePath);

        if (_options.Scanning.Enabled && _virusScanService != null && ShouldScanFile(storedFile))
        {
            await TriggerVirusScanAsync(storedFile, cancellationToken);
        }
        else
        {

            storedFile.MarkScanned(ScanResult.Clean("NoScan", "Scanning disabled"));
        }

        storedFile.MarkUploaded(request.OwnerId);
        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new FileUploadedIntegrationEvent
        {
            FileId = storedFile.Id,
            FileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            Size = storedFile.Size,
            Checksum = storedFile.Checksum,
            OwnerService = storedFile.OwnerService,
            OwnerId = storedFile.OwnerId,
            UploadedBy = storedFile.UploadedBy,
            UploadedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        return FileUploadResult.Ok(storedFile.ToDto());
    }

    public async Task<ScanStatusResponse> CheckScanStatusAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null)
        {
            return new ScanStatusResponse
            {
                FileId = fileId,
                Status = FileStatus.Failed,
                Message = "File not found"
            };
        }

        if (storedFile.Status == FileStatus.Scanned || storedFile.Status == FileStatus.Infected)
        {
            return new ScanStatusResponse
            {
                FileId = fileId,
                Status = storedFile.Status,
                ScanResult = storedFile.ScanResult,
                IsComplete = true,
                Message = storedFile.Status == FileStatus.Infected
                    ? "File is infected and has been quarantined"
                    : "Scan complete"
            };
        }

        if (storedFile.Status == FileStatus.Scanning && _virusScanService != null)
        {
            var scanStatus = await _virusScanService.CheckScanStatusAsync(storedFile.StoragePath, cancellationToken);

            if (scanStatus == ScanStatus.Complete)
            {
                var result = await _virusScanService.ScanFileAsync(storedFile.StoragePath, cancellationToken);

                if (result.IsInfected)
                {
                    storedFile.MarkInfected(result);
                    await QuarantineFileAsync(storedFile, cancellationToken);
                }
                else
                {
                    storedFile.MarkScanned(result);
                }

                _unitOfWork.StoredFiles.Update(storedFile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        return new ScanStatusResponse
        {
            FileId = fileId,
            Status = storedFile.Status,
            ScanResult = storedFile.ScanResult,
            IsComplete = storedFile.Status == FileStatus.Scanned || storedFile.Status == FileStatus.Infected,
            Message = GetScanStatusMessage(storedFile)
        };
    }

    public async Task<SubmitFileResponse> SubmitFileAsync(
        SubmitFileRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(request.FileId, cancellationToken);

        if (storedFile == null)
        {
            return new SubmitFileResponse { Success = false, Error = "File not found" };
        }

        if (storedFile.Status != FileStatus.Scanned && storedFile.Status != FileStatus.Uploaded)
        {
            return new SubmitFileResponse
            {
                Success = false,
                Error = $"File must be scanned clean before submission. Current status: {storedFile.Status}"
            };
        }

        if (storedFile.ScanResult != null && !storedFile.ScanResult.IsClean)
        {
            return new SubmitFileResponse { Success = false, Error = "File scan result indicates infection" };
        }

        var permanentPath = await _storageProvider.MoveAsync(
            storedFile.StoragePath,
            _options.PermanentFolder,
            cancellationToken);

        if (string.IsNullOrEmpty(permanentPath))
        {
            return new SubmitFileResponse { Success = false, Error = "Failed to move file to permanent storage" };
        }

        storedFile.MarkInMedia(permanentPath);

        if (request.ResourceId.HasValue && request.ResourceType.HasValue)
        {
            storedFile.AssociateWithResource(request.ResourceId.Value, request.ResourceType.Value);
        }

        if (!string.IsNullOrEmpty(request.OwnerId))
        {
            storedFile.Confirm(permanentPath, request.OwnerId, request.OwnerService);
        }

        if (request.Permissions != null)
        {
            storedFile.SetPermissions(request.Permissions);
        }

        if (request.Metadata != null)
        {
            storedFile.SetMetadata(request.Metadata);
        }

        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var downloadUrl = await GetDownloadUrlAsync(storedFile.Id, cancellationToken);

        return new SubmitFileResponse
        {
            Success = true,
            File = storedFile.ToDto(),
            DownloadUrl = downloadUrl?.DownloadUrl
        };
    }

    public async Task<FileUploadResult> ConfirmFileAsync(
        FileConfirmRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(request.FileId, cancellationToken);

        if (storedFile == null)
        {
            return FileUploadResult.Fail("File not found");
        }

        if (storedFile.Status != FileStatus.Uploaded && storedFile.Status != FileStatus.Scanned)
        {
            return FileUploadResult.Fail($"File must be uploaded/scanned before confirming. Current status: {storedFile.Status}");
        }

        var newPath = await _storageProvider.MoveAsync(
            storedFile.StoragePath,
            _options.PermanentFolder,
            cancellationToken);

        if (string.IsNullOrEmpty(newPath))
        {
            return FileUploadResult.Fail("Failed to move file to permanent storage");
        }

        storedFile.Confirm(
            newStoragePath: newPath,
            ownerId: request.OwnerId,
            ownerService: request.OwnerService
        );

        if (request.Metadata != null)
        {
            storedFile.SetMetadata(request.Metadata);
        }

        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FileUploadResult.Ok(storedFile.ToDto());
    }

    public async Task<DownloadUrlResponse?> GetDownloadUrlAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || !storedFile.IsAvailableForDownload)
        {
            return null;
        }

        var urlResult = await _storageProvider.GenerateDownloadUrlAsync(
            storedFile.StoragePath,
            _options.DownloadUrlExpirationMinutes,
            cancellationToken);

        if (!urlResult.Success)
        {
            return null;
        }

        return new DownloadUrlResponse
        {
            FileId = storedFile.Id,
            DownloadUrl = urlResult.Url!,
            ExpiresAt = urlResult.ExpiresAt!.Value,
            FileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            Size = storedFile.Size
        };
    }

    public async Task<DownloadUrlResponse?> GetDownloadUrlWithPermissionCheckAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles,
        CancellationToken cancellationToken = default)
    {
        if (_permissionService != null)
        {
            var permissionResult = await _permissionService.CanDownloadAsync(
                fileId,
                userId,
                userRoles?.ToList(),
                cancellationToken);

            if (!permissionResult.IsAllowed)
            {
                _logger.LogWarning(
                    "Download denied for file {FileId} by user {UserId}: {Reason}",
                    fileId, userId, permissionResult.DenialReason);
                return null;
            }
        }

        return await GetDownloadUrlAsync(fileId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Removed)
        {
            return false;
        }

        storedFile.MarkRemoved();

        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new FileDeletedIntegrationEvent
        {
            FileId = storedFile.Id,
            OwnerId = storedFile.OwnerId,
            DeletedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        return true;
    }

    public async Task<FileMetadataDto?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Removed)
        {
            return null;
        }

        return storedFile.ToDto();
    }

    public async Task<IEnumerable<FileMetadataDto>> GetByOwnerAsync(
        string ownerService,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        var files = await _unitOfWork.StoredFiles.GetByOwnerAsync(ownerService, ownerId, cancellationToken);
        return files.Where(f => f.Status == FileStatus.Confirmed || f.Status == FileStatus.InMedia).ToDtoList();
    }

    public async Task<IEnumerable<FileMetadataDto>> GetByResourceAsync(
        Guid resourceId,
        StorageResourceType resourceType,
        CancellationToken cancellationToken = default)
    {
        var files = await _unitOfWork.StoredFiles.GetByResourceAsync(resourceId, resourceType, cancellationToken);
        return files.Where(f => f.IsAvailableForDownload).ToDtoList();
    }

    public async Task<FileUploadResult> DirectUploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string ownerService,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateFile(fileName, contentType, stream.Length);
        if (validationError != null)
        {
            return FileUploadResult.Fail(validationError);
        }

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var uploadResult = await _storageProvider.UploadAsync(
            stream,
            uniqueFileName,
            contentType,
            _options.TempFolder,
            cancellationToken);

        if (!uploadResult.Success)
        {
            return FileUploadResult.Fail(uploadResult.Error ?? "Upload failed");
        }

        var storedFile = StoredFile.Create(
            fileName: uniqueFileName,
            originalFileName: fileName,
            contentType: contentType,
            size: stream.Length,
            provider: GetStorageProvider(),
            storagePath: uploadResult.Path!,
            ownerService: ownerService,
            ownerId: null,
            uploadedBy: uploadedBy,
            bucketName: _options.Azure.TempContainerName,
            expiresIn: TimeSpan.FromHours(_options.TempFileExpirationHours)
        );

        if (!string.IsNullOrEmpty(uploadResult.Checksum))
        {
            storedFile.SetChecksum(uploadResult.Checksum);
        }

        await _unitOfWork.StoredFiles.AddAsync(storedFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new FileUploadedIntegrationEvent
        {
            FileId = storedFile.Id,
            FileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            Size = storedFile.Size,
            Checksum = storedFile.Checksum,
            OwnerService = storedFile.OwnerService,
            OwnerId = storedFile.OwnerId,
            UploadedBy = storedFile.UploadedBy,
            UploadedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        return FileUploadResult.Ok(storedFile.ToDto());
    }

    #region Multipart Upload

    public async Task<MultipartUploadSession?> InitiateMultipartUploadAsync(
        InitiateMultipartUploadRequest request,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        if (request.TotalSize < _options.Multipart.ThresholdBytes)
        {
            _logger.LogWarning(
                "File size {Size} is below multipart threshold {Threshold}. Use regular upload.",
                request.TotalSize, _options.Multipart.ThresholdBytes);
            return null;
        }

        var validationError = ValidateFile(request.FileName, request.ContentType, request.TotalSize);
        if (validationError != null)
        {
            throw new ValidationAppException(validationError, new Dictionary<string, string[]>());
        }

        var partSize = _options.Multipart.PartSize;
        var partsCount = (int)Math.Ceiling((double)request.TotalSize / partSize);

        if (partsCount > _options.Multipart.MaxParts)
        {
            var errorMessage = $"File requires {partsCount} parts, exceeds maximum of {_options.Multipart.MaxParts}";
            throw new ValidationAppException(
                errorMessage,
                new Dictionary<string, string[]> { ["TotalSize"] = [errorMessage] });
        }

        var extension = Path.GetExtension(request.FileName)?.ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var storagePath = _storageProvider.GetStoragePath(_options.TempFolder, uniqueFileName);

        var storedFile = StoredFile.Create(
            fileName: uniqueFileName,
            originalFileName: request.FileName,
            contentType: request.ContentType,
            size: request.TotalSize,
            provider: GetStorageProvider(),
            storagePath: storagePath,
            ownerService: request.OwnerService,
            ownerId: null,
            uploadedBy: uploadedBy,
            bucketName: _options.Azure.TempContainerName,
            expiresIn: TimeSpan.FromDays(_options.Multipart.IncompleteUploadMaxAgeDays),
            resourceType: request.ResourceType
        );

        await _unitOfWork.StoredFiles.AddAsync(storedFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_storageProvider is IAzureBlobStorageProvider azureProvider)
        {
            var uploadInfo = await azureProvider.InitiateMultipartUploadAsync(
                uniqueFileName,
                request.ContentType,
                _options.TempFolder,
                cancellationToken);

            if (string.IsNullOrEmpty(uploadInfo.UploadId))
            {
                storedFile.MarkFailed("Failed to initiate multipart upload");
                _unitOfWork.StoredFiles.Update(storedFile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return null;
            }

            storedFile.SetMultipartUpload(uploadInfo.UploadId, partsCount);
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var parts = new List<UploadPartInfo>();
            for (int i = 1; i <= partsCount; i++)
            {
                var isLastPart = i == partsCount;
                var currentPartSize = isLastPart
                    ? request.TotalSize - (partSize * (partsCount - 1))
                    : partSize;

                var partUrl = azureProvider.GeneratePartUploadUrl(
                    uploadInfo.Container,
                    uploadInfo.Key,
                    uploadInfo.UploadId,
                    i,
                    _options.Multipart.PartUploadUrlExpirationMinutes);

                parts.Add(new UploadPartInfo
                {
                    PartNumber = i,
                    Size = currentPartSize,
                    UploadUrl = partUrl,
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.Multipart.PartUploadUrlExpirationMinutes)
                });
            }

            return new MultipartUploadSession
            {
                FileId = storedFile.Id,
                UploadId = uploadInfo.UploadId,
                Key = storagePath,
                Parts = parts,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.Multipart.IncompleteUploadMaxAgeDays)
            };
        }

        storedFile.MarkFailed("Multipart upload not supported for local storage");
        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return null;
    }

    public async Task<FileUploadResult> CompleteMultipartUploadAsync(
        CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(request.FileId, cancellationToken);

        if (storedFile == null)
        {
            return FileUploadResult.Fail("File not found");
        }

        if (_storageProvider is not IAzureBlobStorageProvider azureProvider)
        {
            return FileUploadResult.Fail("Multipart upload not supported for this storage provider");
        }

        var blockIds = request.Parts
            .OrderBy(p => p.PartNumber)
            .Select(p => Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{request.UploadId}-{p.PartNumber:D5}")))
            .ToList();

        var (containerName, blobPath) = ParseStoragePath(storedFile.StoragePath);

        var url = await azureProvider.CompleteMultipartUploadAsync(
            containerName,
            blobPath,
            request.UploadId,
            blockIds,
            cancellationToken);

        if (string.IsNullOrEmpty(url))
        {
            storedFile.MarkFailed("Failed to complete multipart upload");
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return FileUploadResult.Fail("Failed to complete multipart upload");
        }

        storedFile.CompleteMultipartUpload(request.Parts.Select(p => p.PartNumber).ToList());
        storedFile.MarkInTemp(storedFile.StoragePath);

        if (_options.Scanning.Enabled && _virusScanService != null && ShouldScanFile(storedFile))
        {
            await TriggerVirusScanAsync(storedFile, cancellationToken);
        }
        else
        {
            storedFile.MarkScanned(ScanResult.Clean("NoScan", "Scanning disabled"));
        }

        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FileUploadResult.Ok(storedFile.ToDto());
    }

    #endregion

    #region Private Methods

    private StorageProvider GetStorageProvider()
    {
        return StoragePathHelper.GetStorageProvider(_options.DefaultProvider);
    }

    private string? ValidateFile(string fileName, string contentType, long size)
    {
        return FileValidationHelper.ValidateFile(fileName, contentType, size, _options);
    }

    private bool ShouldScanFile(StoredFile file)
    {
        return VirusScanHelper.ShouldScanFile(
            file.Size,
            file.ContentType,
            _options.Scanning.MaxScanFileSize,
            _options.Scanning.ExemptContentTypes);
    }

    private async Task TriggerVirusScanAsync(StoredFile file, CancellationToken cancellationToken)
    {
        try
        {
            file.MarkScanning();
            _unitOfWork.StoredFiles.Update(file);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _virusScanService!.TriggerScanAsync(file.StoragePath, cancellationToken);

            _logger.LogInformation("Triggered virus scan for file {FileId}", file.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger virus scan for file {FileId}", file.Id);
            file.MarkScanned(ScanResult.Error(ex.Message, "ScanTrigger"));
        }
    }

    private async Task QuarantineFileAsync(StoredFile file, CancellationToken cancellationToken)
    {
        if (_virusScanService == null) return;

        try
        {
            var quarantinePath = await _virusScanService.QuarantineFileAsync(
                file.StoragePath,
                cancellationToken);

            if (!string.IsNullOrEmpty(quarantinePath))
            {
                file.SetQuarantinePath(quarantinePath);
                _logger.LogWarning(
                    "File {FileId} quarantined to {Path}. Threat: {Threat}",
                    file.Id, quarantinePath, file.ScanResult?.ThreatDetails);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to quarantine infected file {FileId}", file.Id);
        }
    }

    private static string GetScanStatusMessage(StoredFile file)
    {
        return VirusScanHelper.GetScanStatusMessage(file);
    }

    private (string containerName, string blobPath) ParseStoragePath(string path)
    {
        return StoragePathHelper.ParseStoragePath(path, _options);
    }

    #endregion
}
