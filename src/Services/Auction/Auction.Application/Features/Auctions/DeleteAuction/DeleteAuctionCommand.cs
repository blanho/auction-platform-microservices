namespace Auctions.Application.Commands.DeleteAuction;

public record DeleteAuctionCommand(Guid Id) : ICommand<bool>;
