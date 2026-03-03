namespace Auctions.Application.Features.Auctions.CancelAuction;

public record CancelAuctionCommand(
    Guid AuctionId,
    Guid UserId,
    string Reason
) : ICommand<bool>;
