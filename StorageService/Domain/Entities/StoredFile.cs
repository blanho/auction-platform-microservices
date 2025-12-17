using Common.Domain.Entities;

namespace StorageService.Domain.Entities;

public class StoredFile : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Path { get; set; } = string.Empty;
    public string Url { get; set; }
    public FileStatus Status { get; set; } = FileStatus.Temporary;
    public string EntityId { get; set; }
    public string EntityType { get; set; }
    public string UploadedBy { get; set; }
    public DateTimeOffset ConfirmedAt { get; set; }
    public string Tags { get; set; }
}
