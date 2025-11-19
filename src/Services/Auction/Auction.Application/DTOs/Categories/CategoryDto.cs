namespace Auctions.Application.DTOs.Categories
{
    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int AuctionCount { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-box";
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public Guid? ParentCategoryId { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Icon { get; set; } = "fa-box";
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }

    public class BulkUpdateCategoriesDto
    {
        public List<Guid> CategoryIds { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class ImportCategoriesDto
    {
        public List<CreateCategoryDto> Categories { get; set; } = new();
    }

    public class ImportCategoriesResultDto
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ExportCategoriesRequestDto
    {
        public string Format { get; set; } = "json";
        public bool? ActiveOnly { get; set; }
    }

    public record CategoryStatDto(Guid CategoryId, string CategoryName, int AuctionCount, decimal Revenue);
}

