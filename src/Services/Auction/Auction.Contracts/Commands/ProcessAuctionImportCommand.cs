namespace AuctionService.Contracts.Commands;

public record ProcessAuctionImportCommand
{
    public Guid? ParentJobId { get; init; }
    public Guid? ParentJobItemId { get; init; }
    public int Attempt { get; init; }
    public string? Culture { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; }
    public List<ImportAuctionItemPayload> Rows { get; init; } = [];
}
