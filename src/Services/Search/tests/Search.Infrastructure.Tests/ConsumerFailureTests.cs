using System.Reflection;
using AuctionService.Contracts.Events;
using BidService.Contracts.Events;
using BidService.Contracts.Constants;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Search.Application.Interfaces;
using Search.Infrastructure.Consumers;
using Xunit;

namespace Search.Infrastructure.Tests;

public class ConsumerFailureTests
{
    public static IEnumerable<object[]> Consumers()
    {
        var pairs = new (Type Consumer, Type Message)[]
        {
            (typeof(AuctionCreatedConsumer), typeof(AuctionCreatedEvent)),
            (typeof(AuctionUpdatedConsumer), typeof(AuctionUpdatedEvent)),
            (typeof(AuctionDeletedConsumer), typeof(AuctionDeletedEvent)),
            (typeof(AuctionFinishedConsumer), typeof(AuctionFinishedEvent)),
            (typeof(HighestBidUpdatedConsumer), typeof(HighestBidUpdatedEvent)),
            (typeof(BidRetractedConsumer), typeof(BidRetractedEvent))
        };
        foreach (var pair in pairs)
        {
            yield return new object[] { pair.Consumer, pair.Message, true };
            yield return new object[] { pair.Consumer, pair.Message, false };
        }
    }

    [Theory]
    [MemberData(nameof(Consumers))]
    public async Task IndexFailure_IsPropagatedForRetry_WhileSuccessCompletes(Type consumerType, Type eventType, bool fail)
    {
        var calls = 0;
        var index = Stub<IAuctionIndexService>((method, _) =>
        {
            Assert.Contains(method.Name, new[] { "IndexAsync", "PartialUpdateAsync", "DeleteAsync", "ApplyBidStateAsync" });
            calls++;
            return Task.FromResult(fail ? Result.Failure(Error.Create("Test.IndexUnavailable", "Unavailable")) : Result.Success());
        });
        var loggerType = typeof(NullLogger<>).MakeGenericType(consumerType);
        var logger = loggerType.GetField("Instance")!.GetValue(null);
        var args = consumerType.GetConstructors().Single().GetParameters().Length == 2
            ? new object?[] { index, logger }
            : new object?[] { index, new DateTimeProvider(), logger };
        var consumer = Activator.CreateInstance(consumerType, args)!;
        var message = Activator.CreateInstance(eventType)!;
        eventType.GetProperty("BidStatus")?.SetValue(message, BidEventStatusNames.Accepted);
        var context = typeof(ConsumerFailureTests).GetMethod(nameof(Context), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(eventType).Invoke(null, new[] { message });
        Task Consume() => (Task)consumerType.GetMethod("Consume")!.Invoke(consumer, new[] { context })!;
        if (fail)
            await Assert.ThrowsAsync<InvalidOperationException>(Consume);
        else
            await Consume();
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task RetractingNonHighestBid_DoesNotChangeIndexedPrice()
    {
        var index = Stub<IAuctionIndexService>((_, _) => throw new InvalidOperationException("No index write expected"));
        var consumer = new BidRetractedConsumer(index, NullLogger<BidRetractedConsumer>.Instance);
        await consumer.Consume(Context(new BidRetractedEvent { WasHighestBid = false }));
    }

    private static ConsumeContext<T> Context<T>(T message) where T : class =>
        Stub<ConsumeContext<T>>((method, _) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            _ => throw new NotSupportedException(method.Name)
        });

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> invoke) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = invoke;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    }
}
