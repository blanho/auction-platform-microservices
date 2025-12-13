using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.DeleteAuction;

public record DeleteAuctionCommand(Guid Id) : ICommand<bool>;
