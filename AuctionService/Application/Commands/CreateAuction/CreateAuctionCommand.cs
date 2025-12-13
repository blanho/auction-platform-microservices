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
    List<Guid>? FileIds = null
) : ICommand<AuctionDto>;
