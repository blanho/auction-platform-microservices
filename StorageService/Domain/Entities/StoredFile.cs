#nullable enable
using Common.Domain.Entities;
using StorageService.Domain.Enums;

namespace StorageService.Domain.Entities;

public class StoredFile : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public StorageProvider Provider { get; set; } = StorageProvider.Local;
    public string StoragePath { get; set; } = string.Empty;
    public string? BucketName { get; set; }
    public string? Checksum { get; set; }
    public string OwnerService { get; set; } = string.Empty;
    public string? OwnerId { get; set; }
    public string? UploadedBy { get; set; }
    public FileStatus Status { get; set; } = FileStatus.Pending;
    public DateTimeOffset? ConfirmedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string? FailureReason { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }

    // Domain behavior methods
    public void Confirm()
    {
        if (Status != FileStatus.Pending)
            throw new InvalidOperationException($"Can only confirm files in Pending status. Current: {Status}");

        Status = FileStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        ExpiresAt = null;
    }

    public void MarkDeleted()
    {
        Status = FileStatus.Deleted;
    }

    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason cannot be empty", nameof(reason));

        Status = FileStatus.Failed;
        FailureReason = reason;
    }

    public void SetMetadata(Dictionary<string, string> metadata)
    {
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    public void AddMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be empty", nameof(key));

        Metadata ??= new Dictionary<string, string>();
        Metadata[key] = value;
    }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;

    public bool CanBeDeleted => Status == FileStatus.Pending || 
                                Status == FileStatus.Failed || 
                                Status == FileStatus.Deleted ||
                                IsExpired;
}
