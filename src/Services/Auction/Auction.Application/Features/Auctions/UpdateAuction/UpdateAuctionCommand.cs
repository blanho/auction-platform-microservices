namespace Auctions.Application.Features.Auctions.UpdateAuction;

public record UpdateAuctionCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Condition,
    int? YearManufactured,
    Dictionary<string, string>? Attributes,
    decimal? ReservePrice,
    decimal? BuyNowPrice,
    Guid? CategoryId,
    Guid? BrandId,
    bool? IsFeatured,
    DateTimeOffset? AuctionEnd,
    Guid UserId
) : ICommand<bool>;
