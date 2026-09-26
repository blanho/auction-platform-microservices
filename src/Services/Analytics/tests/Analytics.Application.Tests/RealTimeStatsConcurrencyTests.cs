using System.Reflection;
using Analytics.Application.Features.PlatformAnalytics.GetRealTimeStats;
using Analytics.Application.Interfaces;
using Xunit;

namespace Analytics.Application.Tests;

public class RealTimeStatsConcurrencyTests
{
    [Fact]
    public async Task Handle_WaitsForAuctionQueryBeforeStartingBidQuery()
    {
        var auctionResult = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bidQueryStarted = false;
        var auctionRepository = CreateProxy<IFactAuctionRepository>((method, _) =>
            method.Name == nameof(IFactAuctionRepository.GetLiveAuctionsCountAsync)
                ? auctionResult.Task
                : throw new NotSupportedException(method.Name));
        var bidRepository = CreateProxy<IFactBidRepository>((method, _) =>
        {
            if (method.Name != nameof(IFactBidRepository.GetBidsInLastHourAsync))
                throw new NotSupportedException(method.Name);

            bidQueryStarted = true;
            return Task.FromResult(7);
        });

        var handler = new GetRealTimeStatsQueryHandler(auctionRepository, bidRepository);
        var resultTask = handler.Handle(new GetRealTimeStatsQuery(), CancellationToken.None);

        Assert.False(bidQueryStarted);
        auctionResult.SetResult(3);

        var result = await resultTask;
        Assert.True(bidQueryStarted);
        Assert.Equal(3, result.ActiveAuctions);
        Assert.Equal(7, result.BidsLastHour);
    }

    private static T CreateProxy<T>(Func<MethodInfo, object?[]?, object?> invoke) where T : class
    {
        var proxy = DispatchProxy.Create<T, RepositoryProxy>();
        ((RepositoryProxy)(object)proxy).InvokeHandler = invoke;
        return proxy;
    }

    public class RepositoryProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> InvokeHandler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            => InvokeHandler(targetMethod!, args);
    }
}
