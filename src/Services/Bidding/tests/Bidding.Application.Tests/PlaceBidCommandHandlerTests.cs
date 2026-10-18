using System.Reflection;
using Bidding.Application.DTOs;
using Bidding.Application.Features.Bids.PlaceBid;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Bidding.Application.Tests;

public class PlaceBidCommandHandlerTests
{
    [Theory]
    [InlineData("Accepted", null, null, null)]
    [InlineData("acceptedbelowreserve", null, null, null)]
    [InlineData("TooLow", null, "Bid.TooLow", "Bid amount is below the required minimum.")]
    [InlineData("Rejected", null, "Bid.Rejected", "Bid was rejected.")]
    [InlineData("Pending", null, "Bid.Rejected", "Bid finished with unsupported status 'Pending'.")]
    [InlineData("unknown", null, "Bid.Rejected", "Bid finished with unsupported status 'unknown'.")]
    [InlineData("999", null, "Bid.Rejected", "Bid finished with unsupported status '999'.")]
    [InlineData("unknown", "Service rejection", "Bid.Rejected", "Service rejection")]
    public async Task BidOutcome_PreservesStatusMappingAndOnlyAuditsAcceptedBids(
        string status, string? serviceError, string? expectedCode, string? expectedDescription)
    {
        var bid = new BidDto { Id = Guid.NewGuid(), Status = status, ErrorMessage = serviceError };
        var removed = false;
        var audited = false;
        var deduplication = Stub<IMessageDeduplicationService>((method, _) => method.Name switch
        {
            nameof(IMessageDeduplicationService.TryMarkAsProcessedAsync) => Task.FromResult(true),
            nameof(IMessageDeduplicationService.RemoveAsync) => Remove(),
            _ => throw new NotSupportedException(method.Name)
        });
        Task Remove()
        {
            removed = true;
            return Task.CompletedTask;
        }
        var bidService = Stub<IBidService>((method, _) => method.Name switch
        {
            nameof(IBidService.PlaceBidAsync) => Task.FromResult(bid),
            _ => throw new NotSupportedException(method.Name)
        });
        var audit = Stub<IAuditPublisher>((method, args) =>
        {
            Assert.Equal(nameof(IAuditPublisher.PublishAsync), method.Name);
            Assert.Equal(bid.Id, args![0]);
            audited = true;
            return Task.CompletedTask;
        });
        var handler = new PlaceBidCommandHandler(bidService, deduplication, audit,
            NullLogger<PlaceBidCommandHandler>.Instance);

        var result = await handler.Handle(
            new PlaceBidCommand(Guid.NewGuid(), 100m, Guid.NewGuid(), "bidder", "request-1"),
            CancellationToken.None);

        Assert.Equal(expectedCode is null, result.IsSuccess);
        Assert.Equal(expectedCode is null, audited);
        Assert.Equal(expectedCode is not null, removed);
        if (result.IsSuccess)
        {
            Assert.Same(bid, result.Value);
        }
        else
        {
            var error = Assert.IsType<Error>(result.Error);
            Assert.Equal(expectedCode, error.Code);
            Assert.Equal(expectedDescription, error.Message);
        }
    }

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
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    }
}
