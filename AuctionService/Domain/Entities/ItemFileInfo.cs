#nullable enable
namespace AuctionService.Domain.Entities;

public class ItemFileInfo
{
    public Guid StorageFileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Url { get; set; }
    public string FileType { get; set; } = "image";
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
