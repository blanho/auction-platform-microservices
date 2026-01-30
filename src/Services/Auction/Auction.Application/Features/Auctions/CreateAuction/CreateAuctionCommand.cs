using Auctions.Application.DTOs;
namespace Auctions.Application.Commands.CreateAuction;

public record CreateAuctionCommand(
    string Title,
    string Description,
    string? Condition,
    int? YearManufactured,
    Dictionary<string, string>? Attributes,
    decimal ReservePrice,
    decimal? BuyNowPrice,
    DateTimeOffset AuctionEnd,
    Guid SellerId,
    string SellerUsername,
    string Currency = "USD",
    List<CreateAuctionFileDto>? Files = null,
    Guid? CategoryId = null,
    Guid? BrandId = null,
    bool IsFeatured = false
) : ICommand<AuctionDto>;

public record CreateAuctionFileDto(
    Guid FileId,
    string FileType,
    int DisplayOrder,
    bool IsPrimary
);

