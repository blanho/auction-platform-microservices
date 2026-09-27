using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using OrchestrationService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Messaging.Consumers;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class BuyNowOrderCancellationTests
{
    [Fact]
    public async Task CanceledOrderRead_DoesNotPublishOrderCreationFailure()
    {
        var message = new CreateBuyNowOrder
        {
            CorrelationId = Guid.NewGuid(),
            AuctionId = Guid.NewGuid()
        };
        var context = Stub<ConsumeContext<CreateBuyNowOrder>>((method, args) => method.Name switch
        {
            "get_Message" => message,
            "Publish" => throw new Xunit.Sdk.XunitException($"Unexpected event: {args![0]?.GetType().Name}"),
            _ => throw new NotSupportedException(method.Name)
        });
        var orders = Stub<IOrderRepository>((method, _) => method.Name switch
        {
            nameof(IOrderRepository.GetByAuctionIdAsync) => Task.FromException<Order?>(new OperationCanceledException()),
            _ => throw new NotSupportedException(method.Name)
        });
        var consumer = new CreateBuyNowOrderConsumer(orders, null!,
            NullLogger<CreateBuyNowOrderConsumer>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => consumer.Consume(context));
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
