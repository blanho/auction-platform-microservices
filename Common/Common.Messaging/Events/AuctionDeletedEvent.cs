namespace Common.Messaging.Events;

public class AuctionDeletedEvent
{
    public Guid Id { get; set; }
    public string Seller { get; set; } = string.Empty;
}
