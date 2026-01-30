using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.DTOs
{
    public class CreateAuctionDto
    {
        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        public string? Condition { get; set; }

        [Range(1900, 2100)]
        public int? YearManufactured { get; set; }

        public Dictionary<string, string>? Attributes { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ReservePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? BuyNowPrice { get; set; }

        public string Currency { get; set; } = "USD";

        [Required]
        public DateTimeOffset AuctionEnd { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public bool IsFeatured { get; set; } = false;
    }
}

