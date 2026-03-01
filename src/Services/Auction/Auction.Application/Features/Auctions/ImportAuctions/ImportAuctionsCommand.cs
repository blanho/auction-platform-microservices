namespace Auctions.Application.Commands.ImportAuctions;

public record ImportAuctionsCommand(
    Guid SellerId,
    string SellerUsername,
    string CorrelationId,
    string Currency,
    IReadOnlyList<ImportAuctionRow> Rows) : ICommand<ImportAuctionsResult>;

public record ImportAuctionRow(
    string Title,
    string Description,
    string? Condition,
    int? YearManufactured,
    decimal ReservePrice,
    decimal? BuyNowPrice,
    DateTimeOffset AuctionEnd,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    Dictionary<string, string>? Attributes = null);

public record ImportAuctionsResult(
    string CorrelationId,
    int TotalRows,
    int SucceededCount,
    int FailedCount,
    int SkippedDuplicateCount,
    TimeSpan Duration,
    IReadOnlyList<ImportRowError> Errors);

public record ImportRowError(
    int RowNumber,
    string Field,
    string ErrorMessage);
