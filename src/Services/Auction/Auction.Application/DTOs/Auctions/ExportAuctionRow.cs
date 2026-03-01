namespace Auctions.Application.DTOs.Auctions;

public record ExportAuctionRow(
    Guid AuctionId,
    string Title,
    string Seller,
    string Status,
    string Currency,
    decimal ReservePrice,
    decimal? CurrentHighBid,
    decimal? SoldAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset AuctionEnd,
    string? Category,
    string? Condition);
