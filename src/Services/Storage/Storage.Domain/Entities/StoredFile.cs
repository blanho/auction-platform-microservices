using Storage.Domain.Enums;

namespace Storage.Domain.Entities;

public class StoredFile : AggregateRoot
{
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? SubFolder { get; set; }
    public Guid? OwnerId { get; set; }
    public FileStatus Status { get; set; } = FileStatus.Pending;
    public StorageProvider Provider { get; set; } = StorageProvider.Local;
    public string? Checksum { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

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

public record FileUploadedDomainEvent(
    Guid FileId,
    string FileName,
    string ContentType,
    long FileSize,
    string Url,
    Guid? OwnerId) : DomainEvent;

public record FileDeletedDomainEvent(
    Guid FileId,
    string FileName,
    Guid? OwnerId) : DomainEvent;
