using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.ActivateAuction;

public record ActivateAuctionCommand(Guid AuctionId) : ICommand<AuctionDto>;
