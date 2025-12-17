using Common.Storage.Events;
using MassTransit;
using Microsoft.Extensions.Options;
using StorageService.Application.Configuration;
using StorageService.Application.DTOs;
using StorageService.Application.Interfaces;
using StorageService.Domain.Entities;
using StorageService.Domain.Enums;

namespace StorageService.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IStorageProvider _storageProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly StorageOptions _options;

    public FileStorageService(
        IStorageProvider storageProvider,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IOptions<StorageOptions> options)
    {
        _storageProvider = storageProvider;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _options = options.Value;
    }

    public async Task<UploadUrlResponse> RequestUploadAsync(
        RequestUploadDto request,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateFile(request.FileName, request.ContentType, request.Size);
        if (validationError != null)
        {
            throw new InvalidOperationException(validationError);
        }

        var extension = Path.GetExtension(request.FileName)?.ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var storagePath = _storageProvider.GetStoragePath(_options.TempFolder, uniqueFileName);

        var storedFile = new StoredFile
        {
            FileName = uniqueFileName,
            OriginalFileName = request.FileName,
            ContentType = request.ContentType,
            Size = request.Size,
            Provider = StorageProvider.Local,
            StoragePath = storagePath,
            OwnerService = request.OwnerService,
            Status = FileStatus.Pending,
            UploadedBy = uploadedBy,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(_options.TempFileExpirationHours),
            Metadata = request.Metadata
        };

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
            storedFile.Status = FileStatus.Failed;
            storedFile.FailureReason = urlResult.Error;
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException(urlResult.Error ?? "Failed to generate upload URL");
        }

        return new UploadUrlResponse
        {
            FileId = storedFile.Id,
            UploadUrl = urlResult.Url!,
            ExpiresAt = urlResult.ExpiresAt!.Value,
            RequiredHeaders = urlResult.RequiredHeaders
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
            storedFile.Status = FileStatus.Failed;
            storedFile.FailureReason = "File not found in storage";
            _unitOfWork.StoredFiles.Update(storedFile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            await _publishEndpoint.Publish(new FileUploadFailedEvent
            {
                FileId = storedFile.Id,
                OwnerService = storedFile.OwnerService,
                Reason = "File not found in storage after upload",
                FailedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
            
            return FileUploadResult.Fail("File was not uploaded to storage");
        }

        if (request.Checksum != null)
        {
            var actualChecksum = await _storageProvider.ComputeChecksumAsync(storedFile.StoragePath, cancellationToken);
            if (actualChecksum != request.Checksum)
            {
                storedFile.Status = FileStatus.Failed;
                storedFile.FailureReason = "Checksum mismatch";
                _unitOfWork.StoredFiles.Update(storedFile);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return FileUploadResult.Fail("Checksum verification failed");
            }
            storedFile.Checksum = actualChecksum;
        }
        else
        {
            storedFile.Checksum = await _storageProvider.ComputeChecksumAsync(storedFile.StoragePath, cancellationToken);
        }

        storedFile.Status = FileStatus.Uploaded;
        storedFile.OwnerId = request.OwnerId;
        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new FileUploadedEvent
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

        return FileUploadResult.Ok(MapToDto(storedFile));
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

        if (storedFile.Status != FileStatus.Uploaded)
        {
            return FileUploadResult.Fail($"File must be uploaded before confirming. Current status: {storedFile.Status}");
        }

        var newPath = await _storageProvider.MoveAsync(
            storedFile.StoragePath,
            _options.PermanentFolder,
            cancellationToken);

        if (string.IsNullOrEmpty(newPath))
        {
            return FileUploadResult.Fail("Failed to move file to permanent storage");
        }

        storedFile.StoragePath = newPath;
        storedFile.Status = FileStatus.Confirmed;
        storedFile.OwnerId = request.OwnerId ?? storedFile.OwnerId;
        storedFile.OwnerService = request.OwnerService;
        storedFile.ConfirmedAt = DateTimeOffset.UtcNow;
        storedFile.ExpiresAt = null;
        
        if (request.Metadata != null)
        {
            storedFile.Metadata = request.Metadata;
        }

        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new FileConfirmedEvent
        {
            FileId = storedFile.Id,
            OwnerService = storedFile.OwnerService,
            OwnerId = storedFile.OwnerId,
            ConfirmedAt = storedFile.ConfirmedAt.Value
        }, cancellationToken);

        return FileUploadResult.Ok(MapToDto(storedFile));
    }

    public async Task<DownloadUrlResponse?> GetDownloadUrlAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Deleted)
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

    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Deleted)
        {
            return false;
        }

        storedFile.Status = FileStatus.Deleted;
        storedFile.DeletedAt = DateTimeOffset.UtcNow;
        
        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new FileDeletedEvent
        {
            FileId = storedFile.Id,
            OwnerService = storedFile.OwnerService,
            OwnerId = storedFile.OwnerId,
            DeletedAt = storedFile.DeletedAt.Value
        }, cancellationToken);

        return true;
    }

    public async Task<FileMetadataDto?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Deleted)
        {
            return null;
        }

        return MapToDto(storedFile);
    }

    public async Task<IEnumerable<FileMetadataDto>> GetByOwnerAsync(
        string ownerService,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        var files = await _unitOfWork.StoredFiles.GetByOwnerAsync(ownerService, ownerId, cancellationToken);
        return files.Where(f => f.Status == FileStatus.Confirmed).Select(MapToDto);
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

        var storedFile = new StoredFile
        {
            FileName = uniqueFileName,
            OriginalFileName = fileName,
            ContentType = contentType,
            Size = stream.Length,
            Provider = StorageProvider.Local,
            StoragePath = uploadResult.Path!,
            Checksum = uploadResult.Checksum,
            OwnerService = ownerService,
            Status = FileStatus.Uploaded,
            UploadedBy = uploadedBy,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(_options.TempFileExpirationHours)
        };

        await _unitOfWork.StoredFiles.AddAsync(storedFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new FileUploadedEvent
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

        return FileUploadResult.Ok(MapToDto(storedFile));
    }

    private string? ValidateFile(string fileName, string contentType, long size)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        
        if (string.IsNullOrEmpty(extension) || !_options.AllowedExtensions.Contains(extension))
        {
            return $"File extension '{extension}' is not allowed";
        }

        if (!_options.AllowedContentTypes.Contains(contentType))
        {
            return $"Content type '{contentType}' is not allowed";
        }

        if (size > _options.MaxFileSize)
        {
            return $"File size exceeds maximum allowed size of {_options.MaxFileSize / 1024 / 1024}MB";
        }

        return null;
    }

    private static FileMetadataDto MapToDto(StoredFile file)
    {
        return new FileMetadataDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            ContentType = file.ContentType,
            Size = file.Size,
            Checksum = file.Checksum,
            Provider = file.Provider,
            Status = file.Status,
            OwnerService = file.OwnerService,
            OwnerId = file.OwnerId,
            UploadedBy = file.UploadedBy,
            CreatedAt = file.CreatedAt,
            ConfirmedAt = file.ConfirmedAt,
            Metadata = file.Metadata
        };
    }
}
