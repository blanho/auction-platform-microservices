#nullable enable
using BuildingBlocks.Domain.Entities;
using Catalog.Domain.Events;

namespace Catalog.Domain.Entities;

/// <summary>
/// Brand aggregate root. Represents a manufacturer or label used to classify auction items.
/// This is reference / taxonomy data managed by admins — it belongs to the Catalog bounded
/// context, NOT to the Auction context.
/// </summary>
public class Brand : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;

    public List<MediaFile> Files { get; private set; } = new();

    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsFeatured { get; private set; }

    private Brand() { }

    public static Brand Create(
        string name,
        string slug,
        string? description = null,
        int displayOrder = 0,
        bool isFeatured = false)
    {
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = description,
            DisplayOrder = displayOrder,
            IsFeatured = isFeatured,
            IsActive = true
        };

        brand.AddDomainEvent(new BrandCreatedDomainEvent
        {
            BrandId = brand.Id,
            Name = brand.Name,
            Slug = brand.Slug
        });

        return brand;
    }

    public void Update(
        string? name = null,
        string? slug = null,
        string? description = null,
        int? displayOrder = null,
        bool? isActive = null,
        bool? isFeatured = null)
    {
        if (name is not null) Name = name;
        if (slug is not null) Slug = slug;
        if (description is not null) Description = description;
        if (displayOrder.HasValue) DisplayOrder = displayOrder.Value;
        if (isActive.HasValue) IsActive = isActive.Value;
        if (isFeatured.HasValue) IsFeatured = isFeatured.Value;

        AddDomainEvent(new BrandUpdatedDomainEvent
        {
            BrandId = Id,
            Name = Name,
            Slug = Slug,
            IsActive = IsActive
        });
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetFeatured(bool featured) => IsFeatured = featured;

    public void AddFile(MediaFile file) => Files.Add(file);

    public void RemoveFile(Guid fileId)
    {
        var file = Files.FirstOrDefault(f => f.FileId == fileId);
        if (file != null)
            Files.Remove(file);
    }
}
