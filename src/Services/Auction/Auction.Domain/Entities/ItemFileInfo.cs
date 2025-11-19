#nullable enable
namespace Auctions.Domain.Entities;

public class ItemFileInfo
{
    public Guid FileId { get; set; }
    public string FileType { get; set; } = "image";
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

