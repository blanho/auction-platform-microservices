#nullable enable
using BuildingBlocks.Domain.Entities;

namespace Auctions.Domain.Entities;

public class Brand : BaseEntity
{
    private string _name = string.Empty;
    private string _slug = string.Empty;

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public string Slug
    {
        get => _slug;
        set => _slug = value.ToLowerInvariant();
    }

    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetFeatured(bool featured) => IsFeatured = featured;
}

