namespace Bidding.Application.Interfaces;

public interface IPaymentStatusClient
{
    Task<IReadOnlyDictionary<Guid, AuctionPaymentStatus>> GetForBuyerAsync(
        Guid buyerId, CancellationToken cancellationToken);
}
