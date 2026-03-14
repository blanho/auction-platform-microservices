using AuctionService.Contracts.Commands;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Auctions.QueueAuctionImport;

public record QueueAuctionImportCommand(
    Guid SellerId,
    string SellerUsername,
    string Currency,
    IReadOnlyList<QueueImportAuctionRow> Rows) : ICommand<BackgroundJobResult>;

public record QueueImportAuctionRow(
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
