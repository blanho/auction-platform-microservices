using Bidding.Application.Interfaces;

namespace Bidding.Infrastructure.Services;

public sealed class AuthoritativeBidReader : IAuthoritativeBidReader
{
    private readonly IBidRepository _bidRepository;

    public AuthoritativeBidReader(IBidRepository bidRepository)
    {
        _bidRepository = bidRepository;
    }

    public async Task<AuthoritativeWinningBid?> GetHighestAcceptedBidAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        var bid = await _bidRepository.GetHighestBidForAuctionAsync(auctionId, cancellationToken);
        return bid is null
            ? null
            : new AuthoritativeWinningBid(
                bid.Id,
                bid.AuctionId,
                bid.BidderId,
                bid.BidderUsername,
                bid.Amount,
                bid.BidTime,
                bid.Status.ToString());
    }
}
