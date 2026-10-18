using System.Reflection;
using System.Text.Json;
using AuctionService.Contracts.Events;
using MassTransit;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Messaging.Consumers;
using Xunit;

namespace Notification.Infrastructure.Tests;

public class AuctionRoomUpdatesTests
{
    [Theory]
    [InlineData("bid", "SendBidPlacedToAuctionRoomAsync")]
    [InlineData("end", "SendAuctionEndedToRoomAsync")]
    [InlineData("extend", "SendAuctionExtendedToRoomAsync")]
    public async Task CommittedAuctionEventsInvalidateTheCorrectRoomIncludingDuplicateDelivery(string kind, string expectedMethod)
    {
        var auctionId = Guid.NewGuid();
        var sends = new List<(string Method, string Room, JsonElement Payload)>();
        var hub = Proxy<INotificationHubService>((method, args) =>
        {
            sends.Add((method.Name, (string)args![0]!, JsonSerializer.SerializeToElement(args[1])));
            return Task.CompletedTask;
        });
        var consumer = new AuctionRoomUpdatesConsumer(hub);
        for (var delivery = 0; delivery < 2; delivery++)
        {
            if (kind == "bid") await consumer.Consume(Context(new AuctionHighBidUpdatedEvent { AuctionId = auctionId, BidAmount = 130 }));
            if (kind == "end") await consumer.Consume(Context(new AuctionFinishedEvent { AuctionId = auctionId, SoldAmount = 250 }));
            if (kind == "extend") await consumer.Consume(Context(new AuctionExtendedEvent { AuctionId = auctionId, NewEndTime = DateTimeOffset.UtcNow.AddMinutes(1) }));
        }
        Assert.Equal(2, sends.Count);
        Assert.All(sends, send =>
        {
            Assert.Equal(expectedMethod, send.Method);
            Assert.Equal(auctionId.ToString(), send.Room);
            Assert.Equal(auctionId, send.Payload.GetProperty("auctionId").GetGuid());
            if (kind == "bid") Assert.Equal(130, send.Payload.GetProperty("amount").GetDecimal());
            if (kind == "end") Assert.Equal("Finished", send.Payload.GetProperty("status").GetString());
            if (kind == "extend") Assert.True(send.Payload.TryGetProperty("auctionEnd", out _));
        });
    }

    private static ConsumeContext<T> Context<T>(T message) where T : class =>
        Proxy<ConsumeContext<T>>((method, _) => method.Name == "get_Message" ? message : throw new NotSupportedException(method.Name));

    private static T Proxy<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
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
