using System.Text.Json;
using Microsoft.Extensions.Options;
using StorageService.Application.Configuration;
using StorageService.Application.DTOs;
using StorageService.Application.Interfaces;
using StorageService.Domain.Entities;

namespace StorageService.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IStorageProvider _storageProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly StorageOptions _options;

    public FileStorageService(
        IStorageProvider storageProvider,
        IUnitOfWork unitOfWork,
        IOptions<StorageOptions> options)
    {
        _storageProvider = storageProvider;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<FileUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        
        if (string.IsNullOrEmpty(extension) || !_options.AllowedExtensions.Contains(extension))
        {
            return FileUploadResult.Fail($"File extension '{extension}' is not allowed");
        }

        if (!_options.AllowedContentTypes.Contains(contentType))
        {
            return FileUploadResult.Fail($"Content type '{contentType}' is not allowed");
        }

        if (stream.Length > _options.MaxFileSize)
        {
            return FileUploadResult.Fail($"File size exceeds maximum allowed size of {_options.MaxFileSize / 1024 / 1024}MB");
        }

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
            Path = uploadResult.Path,
            Url = uploadResult.Url,
            Status = FileStatus.Temporary,
            UploadedBy = uploadedBy
        };

        await _unitOfWork.StoredFiles.AddAsync(storedFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FileUploadResult.Ok(MapToDto(storedFile));
    }

    public async Task<FileUploadResult> ConfirmFileAsync(
        FileConfirmRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(request.FileId, cancellationToken);

        if (storedFile == null || storedFile.Status != FileStatus.Temporary)
        {
            return FileUploadResult.Fail("File not found or already confirmed");
        }

        var newPath = await _storageProvider.MoveAsync(
            storedFile.Path,
            _options.PermanentFolder,
            cancellationToken);

        if (string.IsNullOrEmpty(newPath))
        {
            return FileUploadResult.Fail("Failed to move file to permanent storage");
        }

        storedFile.Path = newPath;
        storedFile.Url = _storageProvider.GetUrl(newPath);
        storedFile.Status = FileStatus.Permanent;
        storedFile.EntityId = request.EntityId;
        storedFile.EntityType = request.EntityType;
        storedFile.ConfirmedAt = DateTimeOffset.UtcNow;
        
        if (request.Tags != null)
        {
            storedFile.Tags = JsonSerializer.Serialize(request.Tags);
        }

        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FileUploadResult.Ok(MapToDto(storedFile));
    }

    public async Task<(Stream Stream, FileMetadataDto Metadata)> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Deleted)
        {
            return (null, null);
        }

        var stream = await _storageProvider.DownloadAsync(storedFile.Path, cancellationToken);
        return (stream, MapToDto(storedFile));
    }

    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Deleted)
        {
            return false;
        }

        await _storageProvider.DeleteAsync(storedFile.Path, cancellationToken);

        storedFile.Status = FileStatus.Deleted;
        storedFile.DeletedAt = DateTimeOffset.UtcNow;
        
        _unitOfWork.StoredFiles.Update(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<FileMetadataDto> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _unitOfWork.StoredFiles.GetByIdAsync(fileId, cancellationToken);

        if (storedFile == null || storedFile.Status == FileStatus.Deleted)
        {
            return null;
        }

        return MapToDto(storedFile);
    }

    public async Task<IEnumerable<FileMetadataDto>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var files = await _unitOfWork.StoredFiles.GetByEntityAsync(entityType, entityId, cancellationToken);
        return files.Where(f => f.Status == FileStatus.Permanent).Select(MapToDto);
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
            Path = file.Path,
            Url = file.Url,
            Status = file.Status,
            EntityId = file.EntityId,
            EntityType = file.EntityType,
            UploadedBy = file.UploadedBy,
            CreatedAt = file.CreatedAt,
            ConfirmedAt = file.ConfirmedAt,
            Tags = !string.IsNullOrEmpty(file.Tags)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(file.Tags)
                : null
        };
    }
}
