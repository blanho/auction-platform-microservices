namespace Bidding.Application.Interfaces;

public interface IAuctionBidLock
{
    Task<T> ExecuteAsync<T>(
        Guid auctionId,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
