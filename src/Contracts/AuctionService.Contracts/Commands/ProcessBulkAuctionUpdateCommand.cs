namespace AuctionService.Contracts.Commands;

public record ProcessBulkAuctionUpdateCommand
{
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public List<Guid> AuctionIds { get; init; } = [];
    public bool Activate { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
}
