namespace Bidding.Application.DTOs
{
    public class PlaceBidDto
    {
        public Guid AuctionId { get; set; }
        public decimal Amount { get; set; }
    }
}
