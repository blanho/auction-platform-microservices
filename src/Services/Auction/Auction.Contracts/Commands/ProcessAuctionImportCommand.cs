namespace AuctionService.Contracts.Commands;

public record ProcessAuctionImportCommand
{
    public Guid CorrelationId { get; init; }
    public Guid SellerId { get; init; }
    public string SellerUsername { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; }
    public List<ImportAuctionItemPayload> Rows { get; init; } = [];
}

public record ImportAuctionItemPayload
{
    public int RowNumber { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Condition { get; init; }
    public int? YearManufactured { get; init; }
    public decimal ReservePrice { get; init; }
    public decimal? BuyNowPrice { get; init; }
    public DateTimeOffset AuctionEnd { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public Dictionary<string, string>? Attributes { get; init; }
}
