using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.DTOs
{
    public class UpdateAuctionDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Condition { get; set; }

        [Range(1900, 2100)]
        public int? YearManufactured { get; set; }

        public Dictionary<string, string>? Attributes { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ReservePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? BuyNowPrice { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public bool? IsFeatured { get; set; }

        public DateTimeOffset? AuctionEnd { get; set; }
    }
}

