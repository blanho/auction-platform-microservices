using BuildingBlocks.Domain.Guards;
using Storage.Domain.Enums;
using Storage.Domain.Events;

namespace Storage.Domain.Entities;

public class StoredFile : AggregateRoot
{
    private StoredFile() { }
    public string FileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? SubFolder { get; private set; }
    public Guid? OwnerId { get; private set; }
    public FileStatus Status { get; private set; } = FileStatus.Pending;
    public StorageProvider Provider { get; private set; } = StorageProvider.Local;
    public string? Checksum { get; private set; }
    public Dictionary<string, string> Metadata { get; private set; } = new();

    public static StoredFile Create(
        string fileName,
        string storedFileName,
        string contentType,
        long fileSize,
        string url,
        string? subFolder,
        Guid? ownerId,
        StorageProvider provider)
    {
        Guard.AgainstNullOrEmpty(fileName, nameof(fileName));
        Guard.AgainstNullOrEmpty(storedFileName, nameof(storedFileName));
        Guard.AgainstNullOrEmpty(contentType, nameof(contentType));
        Guard.AgainstNonPositive(fileSize, nameof(fileSize));
        Guard.AgainstNullOrEmpty(url, nameof(url));

        var file = new StoredFile
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileSize = fileSize,
            Url = url,
            SubFolder = subFolder,
            OwnerId = ownerId,
            Status = FileStatus.Ready,
            Provider = provider,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        file.AddDomainEvent(new FileUploadedDomainEvent(
            file.Id,
            file.FileName,
            file.ContentType,
            file.FileSize,
            file.Url,
            file.OwnerId));

        return file;
    }

    public void MarkAsDeleted(Guid? deletedBy)
    {
        Status = FileStatus.Deleted;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy ?? Guid.Empty;

        AddDomainEvent(new FileDeletedDomainEvent(Id, FileName, OwnerId));
    }
}
