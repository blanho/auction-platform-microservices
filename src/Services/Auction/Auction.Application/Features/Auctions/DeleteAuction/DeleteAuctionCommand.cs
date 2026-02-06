namespace Auctions.Application.Commands.DeleteAuction;

public record DeleteAuctionCommand(Guid Id, Guid UserId) : ICommand<bool>;
