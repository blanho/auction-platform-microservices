using System.Text.Json;
using Common.Messaging.Abstractions;
using Common.Storage.Abstractions;
using Common.Storage.Configuration;
using Common.Storage.Enums;
using Common.Storage.Events;
using Common.Storage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UtilityService.Data;
using UtilityService.Domain.Entities;

namespace UtilityService.Services;

/// <summary>
/// File storage service with temp/permanent workflow
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IStorageProvider _storageProvider;
    private readonly UtilityDbContext _dbContext;
    private readonly IEventPublisher _eventPublisher;
    private readonly StorageOptions _options;

    public FileStorageService(
        IStorageProvider storageProvider,
        UtilityDbContext dbContext,
        IEventPublisher eventPublisher,
        IOptions<StorageOptions> options)
    {
        _storageProvider = storageProvider;
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
        _options = options.Value;
    }

    public async Task<FileUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        string? uploadedBy = null,
        CancellationToken cancellationToken = default)
    {
        // Validate file size
        if (stream.Length > _options.MaxFileSize)
        {
            return FileUploadResult.Fail($"File size exceeds maximum allowed size of {_options.MaxFileSize / 1024 / 1024}MB");
        }

        // Validate extension
        if (_options.AllowedExtensions.Count > 0)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_options.AllowedExtensions.Contains(extension))
            {
                return FileUploadResult.Fail($"File extension '{extension}' is not allowed");
            }
        }

        // Upload to temp folder
        var result = await _storageProvider.UploadAsync(
            stream,
            fileName,
            contentType,
            _options.TempFolder,
            cancellationToken);

        if (!result.Success || result.Metadata == null)
        {
            return result;
        }

        // Save metadata to database
        var storedFile = new StoredFile
        {
            Id = result.Metadata.Id,
            FileName = result.Metadata.FileName,
            OriginalFileName = result.Metadata.OriginalFileName,
            ContentType = result.Metadata.ContentType,
            Size = result.Metadata.Size,
            Path = result.Metadata.Path,
            Url = result.Metadata.Url,
            Status = FileStatus.Temporary,
            UploadedBy = uploadedBy,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.StoredFiles.Add(storedFile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new FileUploadedEvent
        {
            FileId = storedFile.Id,
            FileName = storedFile.FileName,
            ContentType = storedFile.ContentType,
            Size = storedFile.Size,
            TempPath = storedFile.Path,
            UploadedBy = uploadedBy,
            UploadedAt = storedFile.CreatedAt,
            ExpiresAt = DateTime.UtcNow.AddHours(_options.TempFileExpirationHours)
        }, cancellationToken);

        return FileUploadResult.Ok(MapToMetadata(storedFile));
    }

    public async Task<FileUploadResult> ConfirmFileAsync(
        FileConfirmRequest request,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _dbContext.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == request.FileId && f.Status == FileStatus.Temporary, cancellationToken);

        if (storedFile == null)
        {
            return FileUploadResult.Fail("File not found or already confirmed");
        }

        // Move to permanent storage
        var newPath = await _storageProvider.MoveAsync(
            storedFile.Path,
            _options.PermanentFolder,
            cancellationToken);

        if (newPath == null)
        {
            return FileUploadResult.Fail("Failed to move file to permanent storage");
        }

        // Update metadata
        storedFile.Path = newPath;
        storedFile.Url = _storageProvider.GetUrl(newPath);
        storedFile.Status = FileStatus.Permanent;
        storedFile.EntityId = request.EntityId;
        storedFile.EntityType = request.EntityType;
        storedFile.ConfirmedAt = DateTime.UtcNow;
        
        if (request.Tags != null)
        {
            storedFile.Tags = JsonSerializer.Serialize(request.Tags);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new FileConfirmedEvent
        {
            FileId = storedFile.Id,
            FileName = storedFile.FileName,
            PermanentPath = storedFile.Path,
            Url = storedFile.Url,
            EntityId = storedFile.EntityId,
            EntityType = storedFile.EntityType,
            ConfirmedAt = storedFile.ConfirmedAt.Value
        }, cancellationToken);

        return FileUploadResult.Ok(MapToMetadata(storedFile));
    }

    public async Task<(Stream? Stream, FileMetadata? Metadata)> DownloadAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var storedFile = await _dbContext.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.Status != FileStatus.Deleted, cancellationToken);

        if (storedFile == null)
        {
            return (null, null);
        }

        var stream = await _storageProvider.DownloadAsync(storedFile.Path, cancellationToken);
        return (stream, MapToMetadata(storedFile));
    }

    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _dbContext.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.Status != FileStatus.Deleted, cancellationToken);

        if (storedFile == null)
        {
            return false;
        }

        // Delete from storage
        await _storageProvider.DeleteAsync(storedFile.Path, cancellationToken);

        // Mark as deleted in database (soft delete)
        storedFile.Status = FileStatus.Deleted;
        storedFile.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync(new FileDeletedEvent
        {
            FileId = storedFile.Id,
            FileName = storedFile.FileName,
            EntityId = storedFile.EntityId,
            EntityType = storedFile.EntityType,
            DeletedAt = storedFile.DeletedAt.Value
        }, cancellationToken);

        return true;
    }

    public async Task<FileMetadata?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var storedFile = await _dbContext.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == fileId && f.Status != FileStatus.Deleted, cancellationToken);

        return storedFile == null ? null : MapToMetadata(storedFile);
    }

    public async Task<IEnumerable<FileMetadata>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var files = await _dbContext.StoredFiles
            .Where(f => f.EntityType == entityType && f.EntityId == entityId && f.Status == FileStatus.Permanent)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

        return files.Select(MapToMetadata);
    }

    private static FileMetadata MapToMetadata(StoredFile file)
    {
        return new FileMetadata
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
