#nullable enable
using BuildingBlocks.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auctions.Domain.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public List<MediaFile> Files { get; set; } = new();

    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetFeatured(bool featured) => IsFeatured = featured;
}

