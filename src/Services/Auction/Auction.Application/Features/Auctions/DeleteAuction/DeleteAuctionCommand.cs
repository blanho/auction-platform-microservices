namespace Auctions.Application.Features.Auctions.DeleteAuction;

public record DeleteAuctionCommand(Guid Id, Guid UserId) : ICommand<bool>;
