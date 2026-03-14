namespace Auctions.Application.Features.Auctions.ExtendAuction;

public record ExtendAuctionCommand(
    Guid AuctionId,
    Guid UserId,
    int ExtensionMinutes
) : ICommand<DateTimeOffset>;
