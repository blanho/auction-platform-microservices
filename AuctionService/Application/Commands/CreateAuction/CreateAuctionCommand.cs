using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateAuction;

public record CreateAuctionCommand(
    string Title,
    string Description,
    string Make,
    string Model,
    int Year,
    string Color,
    int Mileage,
    int ReservePrice,
    int? BuyNowPrice,
    DateTimeOffset AuctionEnd,
    string Seller,
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
