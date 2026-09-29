using System.Reflection;
using Bidding.Application.DTOs;
using Bidding.Application.Features.Bids.PlaceBid;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Bidding.Application.Tests;

public class PlaceBidCancellationTests
{
    [Fact]
    public async Task CanceledBid_RemovesDeduplicationKeyBeforePropagatingCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var removed = false;
        var deduplication = Stub<IMessageDeduplicationService>((method, args) => method.Name switch
        {
            nameof(IMessageDeduplicationService.TryMarkAsProcessedAsync) => Task.FromResult(true),
            nameof(IMessageDeduplicationService.RemoveAsync) => Remove(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task Remove(object?[] args)
        {
            Assert.Equal(CancellationToken.None, args[1]);
            removed = true;
            return Task.CompletedTask;
        }
        var bidService = Stub<IBidService>((method, args) => method.Name switch
        {
            nameof(IBidService.PlaceBidAsync) => CanceledBid(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task<BidDto> CanceledBid(object?[] args)
        {
            Assert.Equal(cancellation.Token, args[^1]);
            return Task.FromCanceled<BidDto>(cancellation.Token);
        }
        var handler = new PlaceBidCommandHandler(bidService, deduplication, null!,
            NullLogger<PlaceBidCommandHandler>.Instance);
        var command = new PlaceBidCommand(Guid.NewGuid(), 100m, Guid.NewGuid(), "bidder", "request-1");

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.Handle(command, cancellation.Token));

        Assert.True(removed);
    }

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
