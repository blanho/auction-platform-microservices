#nullable enable
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Events;

namespace Catalog.Domain.Entities;

/// <summary>
/// Category aggregate root. Represents a hierarchical classification tree for auction items.
/// Belongs to the Catalog bounded context; not part of the Auction lifecycle.
/// </summary>
public class Category : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Icon { get; private set; } = "fa-box";
    public string? Description { get; private set; }

    public List<MediaFile> Files { get; private set; } = new();

    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }

    private readonly List<Category> _subCategories = new();
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private Category() { }

    public static Category Create(
        string name,
        string slug,
        string icon = "fa-box",
        string? description = null,
        int displayOrder = 0,
        bool isActive = true,
        Guid? parentCategoryId = null)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Icon = icon,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = isActive,
            ParentCategoryId = parentCategoryId
        };

        category.AddDomainEvent(new CategoryCreatedDomainEvent
        {
            CategoryId = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId
        });

        return category;
    }

    public void Update(
        string name,
        string slug,
        string icon,
        string? description,
        int displayOrder,
        bool isActive,
        Guid? parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw new DomainInvariantException("Category cannot be its own parent");

        Name = name;
        Slug = slug;
        Icon = icon;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        ParentCategoryId = parentCategoryId;

        AddDomainEvent(new CategoryUpdatedDomainEvent
        {
            CategoryId = Id,
            Name = Name,
            Slug = Slug,
            IsActive = IsActive
        });
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void SetParentCategory(Category? parent)
    {
        if (parent?.Id == Id)
            throw new DomainInvariantException("Category cannot be its own parent");

        ParentCategoryId = parent?.Id;
        ParentCategory = parent;
    }

    public void AddFile(MediaFile file) => Files.Add(file);

    public void RemoveFile(Guid fileId)
    {
        var file = Files.FirstOrDefault(f => f.FileId == fileId);
        if (file != null)
            Files.Remove(file);
    }
}
