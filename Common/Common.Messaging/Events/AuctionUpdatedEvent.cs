#nullable enable

namespace Common.Messaging.Events;

public class AuctionUpdatedEvent
{
    public Guid Id { get; set; }
    public string Seller { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Color { get; set; }
    public int? Mileage { get; set; }
}
