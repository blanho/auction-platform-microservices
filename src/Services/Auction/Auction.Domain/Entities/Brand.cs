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
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Brand name cannot be empty", nameof(value));
            if (value.Length > 100)
                throw new ArgumentException("Brand name cannot exceed 100 characters", nameof(value));
            _name = value;
        }
    }

    public string Slug
    {
        get => _slug;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Brand slug cannot be empty", nameof(value));
            if (value.Length > 100)
                throw new ArgumentException("Brand slug cannot exceed 100 characters", nameof(value));
            _slug = value.ToLowerInvariant();
        }
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

