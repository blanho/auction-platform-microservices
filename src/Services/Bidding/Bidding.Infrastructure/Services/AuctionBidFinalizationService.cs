using Bidding.Application.Interfaces;

namespace Bidding.Infrastructure.Services;

public sealed class AuctionBidFinalizationService : IAuctionBidFinalizationService
{
    private readonly IAuctionBidLock _auctionBidLock;
    private readonly IAuthoritativeBidReader _bidReader;

    public AuctionBidFinalizationService(
        IAuctionBidLock auctionBidLock,
        IAuthoritativeBidReader bidReader)
    {
        _auctionBidLock = auctionBidLock;
        _bidReader = bidReader;
    }

    public Task<AuthoritativeWinningBid?> FinalizeAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default)
    {
        return _auctionBidLock.ExecuteAsync(
            auctionId,
            lockCancellationToken => _bidReader.GetHighestAcceptedBidAsync(
                auctionId,
                lockCancellationToken),
            cancellationToken);
    }
}
