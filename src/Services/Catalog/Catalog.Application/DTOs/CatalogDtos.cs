namespace Catalog.Application.DTOs;

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
}

public class CreateBrandDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
}

public class UpdateBrandDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsFeatured { get; set; }
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-box";
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
}

public class CategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-box";
    public List<CategoryTreeDto> Children { get; set; } = new();
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Icon { get; set; } = "fa-box";
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentCategoryId { get; set; }
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Icon { get; set; } = "fa-box";
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid? ParentCategoryId { get; set; }
}
