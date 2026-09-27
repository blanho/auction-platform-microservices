using System.Reflection;
using Auctions.Application.Interfaces;
using Auctions.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using OrchestrationService.Contracts.Events;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class BuyNowCancellationTests
{
    [Fact]
    public async Task ReserveCancellation_DoesNotPublishBusinessFailure()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var writeRepository = CanceledWriteRepository(cancellation.Token);
        var consumer = new ReserveAuctionForBuyNowConsumer(writeRepository, null!,
            new DateTimeProvider(), NullLogger<ReserveAuctionForBuyNowConsumer>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consumer.Consume(
            Context(new ReserveAuctionForBuyNow { AuctionId = Guid.NewGuid() }, cancellation.Token)));
    }

    [Fact]
    public async Task CompletionCancellation_DoesNotPublishBusinessFailure()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var writeRepository = CanceledWriteRepository(cancellation.Token);
        var consumer = new CompleteBuyNowAuctionConsumer(writeRepository, null!,
            new DateTimeProvider(), NullLogger<CompleteBuyNowAuctionConsumer>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consumer.Consume(
            Context(new CompleteBuyNowAuction { AuctionId = Guid.NewGuid() }, cancellation.Token)));
    }

    [Fact]
    public async Task ReleaseCancellation_ForwardsTokenWithoutPublishing()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var writeRepository = CanceledWriteRepository(cancellation.Token);
        var consumer = new ReleaseAuctionReservationConsumer(writeRepository, null!,
            new DateTimeProvider(), NullLogger<ReleaseAuctionReservationConsumer>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consumer.Consume(
            Context(new ReleaseAuctionReservation { AuctionId = Guid.NewGuid() }, cancellation.Token)));
    }

    private static IAuctionWriteRepository CanceledWriteRepository(CancellationToken cancellationToken) =>
        Stub<IAuctionWriteRepository>((method, args) => method.Name switch
        {
            nameof(IAuctionWriteRepository.GetByIdForUpdateAsync) => CanceledRead(args!, cancellationToken),
            _ => throw new NotSupportedException(method.Name)
        });

    private static Task<AuctionEntity?> CanceledRead(object?[] args, CancellationToken expectedToken)
    {
        var token = (CancellationToken)args[1]!;
        Assert.Equal(expectedToken, token);
        return Task.FromCanceled<AuctionEntity?>(token);
    }

    private static ConsumeContext<T> Context<T>(T message, CancellationToken token)
        where T : class =>
        Stub<ConsumeContext<T>>((method, args) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => token,
            "Publish" => throw new Xunit.Sdk.XunitException($"Unexpected event: {args![0]?.GetType().Name}"),
            _ => throw new NotSupportedException(method.Name)
        });

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
