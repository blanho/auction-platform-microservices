using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.DTOs.Auctions;

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

    public List<CreateAuctionFileInputDto>? Files { get; set; }

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

public class CreateAuctionFileInputDto
{
    [Required]
    public Guid FileId { get; set; }
    public string FileType { get; set; } = "image";
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
}

