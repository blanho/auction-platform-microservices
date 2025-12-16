#nullable enable

namespace Common.Messaging.Events;

public class AuctionUpdatedEvent
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Condition { get; set; }
    public int? YearManufactured { get; set; }
}
