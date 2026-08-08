namespace Bidding.Application.Interfaces;

public sealed record AuthoritativeWinningBid(
    Guid BidId,
    Guid AuctionId,
    Guid BidderId,
    string BidderUsername,
    decimal Amount,
    DateTimeOffset BidTime,
    string Status);

public interface IAuthoritativeBidReader
{
    Task<AuthoritativeWinningBid?> GetHighestAcceptedBidAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);
}

public interface IAuctionBidFinalizationService
{
    Task<AuthoritativeWinningBid?> FinalizeAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);
}
