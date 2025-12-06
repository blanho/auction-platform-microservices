using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateAuction;

/// <summary>
/// Command to create a new auction
/// </summary>
public record CreateAuctionCommand(
    string Title,
    string Description,
    string Make,
    string Model,
    int Year,
    string Color,
    int Mileage,
    int ReservePrice,
    DateTimeOffset AuctionEnd,
    string Seller,
    List<Guid>? FileIds = null
) : ICommand<AuctionDto>;
