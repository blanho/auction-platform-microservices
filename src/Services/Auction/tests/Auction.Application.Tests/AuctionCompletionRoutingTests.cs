using AuctionService.Contracts.Events;
using Auctions.Application.EventHandlers;
using Auctions.Domain.Events;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using OrchestrationService.Contracts.Events;
using Xunit;

namespace Auction.Application.Tests;

public sealed class AuctionCompletionRoutingTests
{
    [Theory]
    [InlineData(true, false, true, true)]
    [InlineData(false, false, true, false)]
    [InlineData(true, true, true, false)]
    [InlineData(true, false, false, false)]
    public async Task CompletionStartsOneWinnerSagaOnlyForAnEligibleSale(
        bool itemSold, bool isBuyNow, bool hasWinner, bool startsWinnerSaga)
    {
        var publisher = new RecordingPublisher();
        var handler = new AuctionFinishedDomainEventHandler(
            publisher, NullLogger<AuctionFinishedDomainEventHandler>.Instance);
        var notification = new AuctionFinishedDomainEvent
        {
            AuctionId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            SellerUsername = "seller",
            WinnerId = hasWinner ? Guid.NewGuid() : null,
            WinnerUsername = hasWinner ? "winner" : null,
            SoldAmount = itemSold ? 130 : null,
            ItemTitle = "auction item",
            ItemSold = itemSold,
            IsBuyNow = isBuyNow
        };
        using var cancellation = new CancellationTokenSource();

        await handler.Handle(notification, cancellation.Token);

        var completed = Assert.Single(publisher.Messages.OfType<AuctionFinishedEvent>());
        Assert.True(completed.OrderCreationManaged);
        Assert.Equal(notification.AuctionId, completed.AuctionId);
        Assert.Equal(isBuyNow, completed.IsBuyNow);
        Assert.Equal(itemSold, completed.ItemSold);
        Assert.Equal(notification.WinnerId, completed.WinnerId);
        Assert.Equal(notification.SoldAmount, completed.SoldAmount);
        Assert.All(publisher.Tokens, token => Assert.Equal(cancellation.Token, token));

        if (!startsWinnerSaga)
        {
            Assert.Single(publisher.Messages);
            return;
        }

        Assert.Equal(2, publisher.Messages.Count);
        var started = Assert.Single(publisher.Messages.OfType<AuctionCompletionSagaStarted>());
        Assert.Equal(notification.AuctionId, started.CorrelationId);
        Assert.Equal(notification.AuctionId, started.AuctionId);
        Assert.Equal(notification.WinnerId, started.WinnerId);
        Assert.Equal(notification.WinnerUsername, started.WinnerUsername);
        Assert.Equal(notification.SellerId, started.SellerId);
        Assert.Equal(notification.SellerUsername, started.SellerUsername);
        Assert.Equal(notification.ItemTitle, started.ItemTitle);
        Assert.Equal(notification.SoldAmount, started.WinningBidAmount);
    }

    private sealed class RecordingPublisher : IEventPublisher
    {
        public List<object> Messages { get; } = [];
        public List<CancellationToken> Tokens { get; } = [];

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            Messages.Add(message);
            Tokens.Add(cancellationToken);
            return Task.CompletedTask;
        }

        public Task PublishBatchAsync<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default) where T : class =>
            throw new NotSupportedException("Auction completion publishes individually through the outbox.");
    }
}
