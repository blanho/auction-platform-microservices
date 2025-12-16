using System.ComponentModel.DataAnnotations;

namespace AuctionService.Application.DTOs;

public class CreateAuctionWithFileIdsDto
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    public string? Condition { get; set; }

    [Range(1900, 2100)]
    public int? YearManufactured { get; set; }

    public Dictionary<string, string>? Attributes { get; set; }

    public List<Guid>? FileIds { get; set; }

    public List<FileInfoDto>? Files { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal ReservePrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? BuyNowPrice { get; set; }

    [Required]
    public DateTimeOffset AuctionEnd { get; set; }

    public Guid? CategoryId { get; set; }

    public bool IsFeatured { get; set; } = false;

    public string Currency { get; set; } = "USD";
}

public class FileInfoDto
{
    public required string Url { get; set; }
    public required string PublicId { get; set; }
    public required string FileName { get; set; }
    public string ContentType { get; set; } = "image/jpeg";
    public long Size { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}
