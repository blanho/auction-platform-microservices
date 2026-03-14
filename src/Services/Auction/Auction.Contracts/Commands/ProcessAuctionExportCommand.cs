namespace AuctionService.Contracts.Commands;

public record ProcessAuctionExportCommand
{
    public Guid CorrelationId { get; init; }
    public Guid RequestedBy { get; init; }
    public string Format { get; init; } = string.Empty;
    public string? StatusFilter { get; init; }
    public string? SellerFilter { get; init; }
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public DateTimeOffset RequestedAt { get; init; }
}
