namespace Bidding.Application.DTOs
{
    public class BidDto
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public Guid BidderId { get; set; }
        public string BidderUsername { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTimeOffset BidTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public decimal MinimumNextBid { get; set; }
        public decimal MinimumIncrement { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}

