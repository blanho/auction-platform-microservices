using System.Reflection;
using Bidding.Application.EventHandlers;
using Bidding.Domain.Events;
using BidService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Bidding.Application.Tests;

public class BidEventOrderingTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Retraction_PreservesOriginalTimestampAndHighestBidFlag(bool wasHighest)
    {
        var (publisher, messages) = Publisher();
        var notification = new BidRetractedDomainEvent { WasHighestBid = wasHighest };
        var handler = new BidRetractedDomainEventHandler(publisher, NullLogger<BidRetractedDomainEventHandler>.Instance);
        await handler.Handle(notification, default);
        await handler.Handle(notification, default);
        Assert.Equal(2, messages.Count);
        Assert.All(messages, message =>
        {
            var retraction = Assert.IsType<BidRetractedEvent>(message);
            Assert.Equal(notification.OccurredAt, retraction.RetractedAt);
            Assert.Equal(wasHighest, retraction.WasHighestBid);
        });
    }

    [Fact]
    public async Task HighestBid_PreservesOriginalTimestampWhenRepublished()
    {
        var (publisher, messages) = Publisher();
        var notification = new HighestBidUpdatedDomainEvent { IsAutoBid = true };
        var handler = new HighestBidUpdatedDomainEventHandler(publisher, NullLogger<HighestBidUpdatedDomainEventHandler>.Instance);
        await handler.Handle(notification, default);
        await handler.Handle(notification, default);
        Assert.Equal(2, messages.Count);
        Assert.All(messages, message => Assert.Equal(notification.OccurredAt, Assert.IsType<HighestBidUpdatedEvent>(message).BidTime));
    }

    private static (IEventPublisher, List<object>) Publisher()
    {
        var publisher = DispatchProxy.Create<IEventPublisher, PublisherProxy>();
        return (publisher, ((PublisherProxy)(object)publisher).Messages);
    }

    public class PublisherProxy : DispatchProxy
    {
        public List<object> Messages { get; } = new();
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            Assert.Equal("PublishAsync", targetMethod?.Name);
            Messages.Add(args![0]!);
            return Task.CompletedTask;
        }
    }
}
