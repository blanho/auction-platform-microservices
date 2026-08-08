namespace Auctions.Application.Interfaces;

public sealed record WinningBidResult(Guid BidderId, string BidderUsername, decimal Amount);

public interface IBidFinalizationClient
{
    Task<WinningBidResult?> FinalizeAuctionAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);
}
