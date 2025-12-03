namespace BidService.Application.DTOs
{
    public class BidDto
    {
        public Guid Id { get; set; }
        public Guid AuctionId { get; set; }
        public string Bidder { get; set; } = string.Empty;
        public int Amount { get; set; }
        public DateTime BidTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
