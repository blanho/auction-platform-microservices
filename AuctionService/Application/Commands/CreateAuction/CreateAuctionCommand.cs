using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateAuction;

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
    List<Guid>? FileIds = null,
    List<FileInfoDto>? Files = null,
    Guid? CategoryId = null,
    bool IsFeatured = false
) : ICommand<AuctionDto>;

public record FileInfoDto(
    string Url,
    string PublicId,
    string FileName,
    string ContentType,
    long Size,
    int DisplayOrder,
    bool IsPrimary
);
