namespace AuctionService.Contracts.Commands;

public record ProcessAuctionImportBatchCommand
{
    public Guid CorrelationId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public int BatchNumber { get; init; }
    public int TotalBatches { get; init; }
    public int TotalRows { get; init; }
    public List<ImportAuctionItemPayload> Rows { get; init; } = [];
}
