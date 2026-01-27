#nullable enable
namespace Auctions.Domain.Entities;

public class MediaFile
{
    public Guid FileId { get; set; }
    public string FileType { get; set; } = "image";
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}
