#nullable enable
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auctions.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string Icon { get; set; } = "fa-box";
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public List<MediaFile> Files { get; set; } = new();

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

