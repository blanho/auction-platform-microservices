using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.DeactivateAuction;

public record DeactivateAuctionCommand(
    Guid AuctionId,
    string? Reason = null
) : ICommand<AuctionDto>;
