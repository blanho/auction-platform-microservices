namespace Payment.Infrastructure.Persistence;

public class BuyNowOrderAttempt
{
    public Guid CorrelationId { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BuyerId { get; set; }
    public bool Cancelled { get; set; }
}
