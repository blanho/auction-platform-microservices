using Common.Storage.Enums;

namespace UtilityService.Domain.Entities;

/// <summary>
/// Entity for storing file metadata in the database
/// </summary>
public class StoredFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Url { get; set; }
    public FileStatus Status { get; set; } = FileStatus.Temporary;
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? Tags { get; set; } // JSON serialized
}
