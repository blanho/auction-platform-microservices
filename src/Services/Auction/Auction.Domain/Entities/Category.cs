#nullable enable
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Auctions.Domain.Entities;

public class Category : BaseEntity
{
    private string _name = string.Empty;
    private string _slug = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Category name cannot be empty", nameof(value));
            if (value.Length > 100)
                throw new ArgumentException("Category name cannot exceed 100 characters", nameof(value));
            _name = value;
        }
    }

    public string Slug
    {
        get => _slug;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Category slug cannot be empty", nameof(value));
            if (value.Length > 100)
                throw new ArgumentException("Category slug cannot exceed 100 characters", nameof(value));
            _slug = value.ToLowerInvariant();
        }
    }

    public string Icon { get; set; } = "fa-box";
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void SetParentCategory(Category? parent)
    {
        if (parent?.Id == Id)
            throw new DomainInvariantException("Category cannot be its own parent");

        ParentCategoryId = parent?.Id;
        ParentCategory = parent;
    }
}

