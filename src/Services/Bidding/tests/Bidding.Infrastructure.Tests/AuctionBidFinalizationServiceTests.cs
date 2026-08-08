using Bidding.Application.Interfaces;
using Bidding.Infrastructure.Services;
using Xunit;

namespace Bidding.Infrastructure.Tests;

public sealed class AuctionBidFinalizationServiceTests
{
    [Fact]
    public async Task FinalizeAsync_ReadsWinningBidInsideAuctionLock()
    {
        var auctionId = Guid.NewGuid();
        var lockManager = new TestAuctionBidLock();
        var bidReader = new AssertingBidReader(() => Assert.True(lockManager.IsHeld));
        var service = new AuctionBidFinalizationService(lockManager, bidReader);

        var result = await service.FinalizeAsync(auctionId);

        Assert.NotNull(result);
        Assert.Equal(auctionId, result.AuctionId);
        Assert.False(lockManager.IsHeld);
    }

    [Fact]
    public async Task FinalizeAsync_SerializesConcurrentRequestsForSameAuction()
    {
        var auctionId = Guid.NewGuid();
        var lockManager = new TestAuctionBidLock();
        var bidReader = new ConcurrencyTrackingBidReader();
        var service = new AuctionBidFinalizationService(lockManager, bidReader);

        await Task.WhenAll(
            service.FinalizeAsync(auctionId),
            service.FinalizeAsync(auctionId));

        Assert.Equal(1, bidReader.MaximumConcurrency);
    }

    private sealed class TestAuctionBidLock : IAuctionBidLock
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public bool IsHeld { get; private set; }

        public async Task<T> ExecuteAsync<T>(
            Guid auctionId,
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);
            IsHeld = true;
            try
            {
                return await operation(cancellationToken);
            }
            finally
            {
                IsHeld = false;
                _semaphore.Release();
            }
        }
    }

    private sealed class AssertingBidReader : IAuthoritativeBidReader
    {
        public AssertingBidReader(Action assertLock) => AssertLock = assertLock;
        public Action AssertLock { get; set; }

        public Task<AuthoritativeWinningBid?> GetHighestAcceptedBidAsync(
            Guid auctionId,
            CancellationToken cancellationToken = default)
        {
            AssertLock();
            return Task.FromResult<AuthoritativeWinningBid?>(CreateWinningBid(auctionId));
        }
    }

    private sealed class ConcurrencyTrackingBidReader : IAuthoritativeBidReader
    {
        private int _currentConcurrency;
        public int MaximumConcurrency { get; private set; }

        public async Task<AuthoritativeWinningBid?> GetHighestAcceptedBidAsync(
            Guid auctionId,
            CancellationToken cancellationToken = default)
        {
            var current = Interlocked.Increment(ref _currentConcurrency);
            MaximumConcurrency = Math.Max(MaximumConcurrency, current);
            try
            {
                await Task.Delay(25, cancellationToken);
                return CreateWinningBid(auctionId);
            }
            finally
            {
                Interlocked.Decrement(ref _currentConcurrency);
            }
        }
    }

    private static AuthoritativeWinningBid CreateWinningBid(Guid auctionId) => new(
        Guid.NewGuid(),
        auctionId,
        Guid.NewGuid(),
        "bidder",
        120m,
        DateTime.UtcNow,
        "Accepted");
}
