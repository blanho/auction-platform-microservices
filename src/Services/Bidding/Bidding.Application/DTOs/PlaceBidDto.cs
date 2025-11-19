using System.ComponentModel.DataAnnotations;

namespace Bidding.Application.DTOs
{
    public class PlaceBidDto
    {
        [Required]
        public Guid AuctionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        public decimal Amount { get; set; }
    }
}

