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
}
