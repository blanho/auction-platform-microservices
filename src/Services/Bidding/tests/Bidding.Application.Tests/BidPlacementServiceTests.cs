using System.Reflection;
using Bidding.Application.DTOs;
using Bidding.Application.Interfaces;
using Bidding.Application.Services;
using Bidding.Domain.Entities;
using Bidding.Domain.Enums;
using Bidding.Domain.Events;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Bidding.Application.Tests;

public class BidPlacementServiceTests
{
    [Theory]
    [InlineData(0, false, false, BidStatus.Accepted)]
    [InlineData(120, false, true, BidStatus.Accepted)]
    [InlineData(150, false, false, BidStatus.AcceptedBelowReserve)]
    [InlineData(0, true, true, BidStatus.Accepted)]
    [InlineData(120, true, false, BidStatus.Accepted)]
    [InlineData(150, true, true, BidStatus.AcceptedBelowReserve)]
    public async Task PlaceBid_PreservesAcceptanceAndPreviousBidMetadata(
        int reservePrice, bool hasPreviousBid, bool isAutoBid, BidStatus expectedStatus)
    {
        var scenario = new BidScenario(reservePrice, hasPreviousBid);

        var result = await scenario.Service.PlaceBidAsync(
            scenario.Request, scenario.BidderId, "bidder", isAutoBid, scenario.Token);

        Assert.Equal(expectedStatus.ToString(), result.Status);
        var bid = Assert.IsType<Bid>(scenario.SavedBid);
        Assert.Equal(expectedStatus, bid.Status);
        Assert.Equal(bid.Id, result.Id);
        var highestBidEvent = Assert.Single(bid.DomainEvents.OfType<HighestBidUpdatedDomainEvent>());
        Assert.Equal(scenario.PreviousBid?.Amount, highestBidEvent.PreviousHighestAmount);
        Assert.Equal(scenario.PreviousBid?.BidderId, highestBidEvent.PreviousBidderId);
        Assert.Equal(scenario.PreviousBid?.BidderUsername, highestBidEvent.PreviousBidderUsername);
        Assert.Equal(isAutoBid, highestBidEvent.IsAutoBid);
        Assert.Equal(new[] { "acquire", "enter", "snapshot", "highest", "create", "save", "exit", "release" }, scenario.Calls);
    }

    [Theory]
    [InlineData(true, 0, BidStatus.TooLow, "Bid amount must be greater than zero.")]
    [InlineData(false, 120, BidStatus.Rejected, "Another bid is being processed. Please try again.")]
    public async Task PlaceBid_PreservesUnsuccessfulResponseWithoutSaving(
        bool lockAvailable, int amount, BidStatus expectedStatus, string expectedError)
    {
        var scenario = new BidScenario(0, false, lockAvailable);
        scenario.Request.Amount = amount;

        var result = await scenario.Service.PlaceBidAsync(
            scenario.Request, scenario.BidderId, "bidder", scenario.Token);

        Assert.Equal(expectedStatus.ToString(), result.Status);
        Assert.Equal(expectedError, result.ErrorMessage);
        Assert.Equal(scenario.Request.AuctionId, result.AuctionId);
        Assert.Equal(scenario.BidderId, result.BidderId);
        Assert.Equal("bidder", result.BidderUsername);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(new DateTimeOffset(BidScenario.Now), result.BidTime);
        Assert.Equal(Guid.Empty, result.Id);
        Assert.Null(scenario.SavedBid);
        Assert.DoesNotContain("save", scenario.Calls);
    }

    private sealed class BidScenario
    {
        public static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        public PlaceBidDto Request { get; } = new() { AuctionId = Guid.NewGuid(), Amount = 120m };
        public Guid BidderId { get; } = Guid.NewGuid();
        public CancellationToken Token { get; } = new CancellationTokenSource().Token;
        public List<string> Calls { get; } = [];
        public Bid? PreviousBid { get; }
        public Bid? SavedBid { get; private set; }
        public BidPlacementService Service { get; }

        public BidScenario(int reservePrice, bool hasPreviousBid, bool lockAvailable = true)
        {
            PreviousBid = hasPreviousBid
                ? Bid.Create(Request.AuctionId, Guid.NewGuid(), "previous-bidder", 100m, Now.AddMinutes(-1))
                : null;
            var repository = Proxy<IBidRepository>((method, args) =>
            {
                Assert.Equal(Token, args[^1]);
                switch (method.Name)
                {
                    case nameof(IBidRepository.GetHighestBidForAuctionAsync):
                        Calls.Add("highest");
                        return Task.FromResult(PreviousBid);
                    case nameof(IBidRepository.CreateAsync):
                        Calls.Add("create");
                        SavedBid = (Bid)args[0]!;
                        return Task.FromResult(SavedBid);
                    default:
                        throw new InvalidOperationException(method.Name);
                }
            });
            var snapshotRepository = Proxy<IAuctionSnapshotRepository>((method, args) =>
            {
                Assert.Equal(nameof(IAuctionSnapshotRepository.GetAsync), method.Name);
                Assert.Equal(Token, args[^1]);
                Calls.Add("snapshot");
                return Task.FromResult<AuctionSnapshot?>(new AuctionSnapshot(
                    Request.AuctionId, "Auction", "seller", Guid.NewGuid(), Now.AddHours(1), "Live", reservePrice, null));
            });
            var unitOfWork = Proxy<IUnitOfWork>((method, args) =>
            {
                Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
                Assert.Equal(Token, args[^1]);
                Calls.Add("save");
                return Task.FromResult(1);
            });
            var distributedLock = Proxy<IDistributedLock>((method, args) =>
            {
                Assert.Equal(nameof(IDistributedLock.TryAcquireAsync), method.Name);
                Assert.Equal(Token, args[^1]);
                Calls.Add("acquire");
                return Task.FromResult<IAsyncDisposable?>(lockAvailable ? new LockHandle(Calls) : null);
            });
            var clock = Proxy<IDateTimeProvider>((method, _) =>
            {
                Assert.Equal("get_UtcNow", method.Name);
                return Now;
            });
            var grpcClient = Proxy<IAuctionGrpcClient>((method, _) => throw new InvalidOperationException(method.Name));
            Service = new BidPlacementService(repository, snapshotRepository,
                NullLogger<BidPlacementService>.Instance, clock, unitOfWork, distributedLock,
                grpcClient, new AuctionLock(Calls));
        }
    }

    private sealed class AuctionLock(List<string> calls) : IAuctionBidLock
    {
        public async Task<T> ExecuteAsync<T>(Guid auctionId, Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            calls.Add("enter");
            try
            {
                return await operation(cancellationToken);
            }
            finally
            {
                calls.Add("exit");
            }
        }
    }

    private sealed class LockHandle(List<string> calls) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            calls.Add("release");
            return ValueTask.CompletedTask;
        }
    }

    private static T Proxy<T>(Func<MethodInfo, object?[], object?> invoke) where T : class
    {
        var proxy = DispatchProxy.Create<T, StubProxy>();
        ((StubProxy)(object)proxy).Handler = invoke;
        return proxy;
    }

    public class StubProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[], object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            Handler(targetMethod!, args!);
    }
}
