using Auctions.Application.DTOs;
using Auctions.Application.Errors;
using Auctions.Application.Features.Auctions.BuyNow;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using OrchestrationService.Contracts.Events;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Application.Tests;

public class BuyNowHandlerTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ExistingCorrelationIsReplayedOnlyForTheSameBuyerAndAuction(bool differentBuyer, bool differentAuction)
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new PurchaseFixture(cancellation.Token);
        var purchase = fixture.ExistingPurchase();
        if (differentBuyer) purchase.BuyerId = Guid.NewGuid();
        if (differentAuction) purchase.AuctionId = Guid.NewGuid();
        fixture.Purchases.Add(purchase.CorrelationId, purchase);

        var result = await fixture.Handler.Handle(fixture.Request, cancellation.Token);

        Assert.Equal(!differentBuyer && !differentAuction, result.IsSuccess);
        if (result.IsSuccess)
        {
            Assert.Equal(purchase.CorrelationId, result.Value!.CorrelationId);
        }
        else
        {
            Assert.Equal(AuctionErrors.BuyNow.Conflict, result.Error);
        }
        Assert.Empty(fixture.Writes);
        Assert.True(fixture.LockReleased);
    }

    [Fact]
    public async Task RetryWithANewKeyReturnsTheBuyersActiveReservation()
    {
        var fixture = new PurchaseFixture();
        var purchase = fixture.ExistingPurchase();
        fixture.Auction.ReserveBuyNow(purchase.CorrelationId, purchase.BuyerId);
        fixture.Purchases.Add(purchase.CorrelationId, purchase);

        var result = await fixture.Handler.Handle(fixture.Request with { CorrelationId = Guid.NewGuid() }, default);

        Assert.True(result.IsSuccess);
        Assert.Equal(purchase.CorrelationId, result.Value!.CorrelationId);
        Assert.Empty(fixture.Writes);
        Assert.True(fixture.LockReleased);
    }

    [Fact]
    public async Task NewPurchaseReservesPublishesAndAuditsBeforeSaving()
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new PurchaseFixture(cancellation.Token);

        var result = await fixture.Handler.Handle(fixture.Request, cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "purchase", "publish", "update", "audit", "save" }, fixture.Writes);
        Assert.Equal(Status.ReservedForBuyNow, fixture.Auction.Status);
        Assert.Equal(fixture.Request.CorrelationId, fixture.Auction.BuyNowCorrelationId);
        Assert.Equal(fixture.Request.BuyerId, fixture.Auction.BuyNowBuyerId);
        Assert.Equal(fixture.Request.CorrelationId, fixture.Published!.CorrelationId);
        Assert.Equal(fixture.Request.BuyerId, fixture.Published.BuyerId);
        Assert.Equal(fixture.Auction.BuyNowPrice, fixture.Published.BuyNowPrice);
        Assert.Equal(nameof(Status.Live), fixture.PreviousAudit!.Status);
        Assert.Equal(nameof(Status.ReservedForBuyNow), fixture.CurrentAudit!.Status);
        Assert.True(fixture.LockReleased);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("audit")]
    public async Task PublicationFailureDoesNotSaveAndAlwaysReleasesLock(string failureStage)
    {
        var fixture = new PurchaseFixture { FailureStage = failureStage };

        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Handler.Handle(fixture.Request, default));

        Assert.DoesNotContain("save", fixture.Writes);
        Assert.True(fixture.LockReleased);
    }

    [Fact]
    public async Task CancellationPropagatesAndReleasesTheLock()
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new PurchaseFixture(cancellation.Token) { CancelRead = true };
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Handler.Handle(fixture.Request, cancellation.Token));

        Assert.Empty(fixture.Writes);
        Assert.True(fixture.LockReleased);
    }

    private sealed class PurchaseFixture
    {
        public AuctionEntity Auction { get; } = AuctionEntity.Create(new CreateAuctionParams(
            Guid.NewGuid(), "seller", Item.Create("item", "description"), 10,
            DateTimeOffset.UtcNow.AddDays(1), BuyNowPrice: 100));
        public BuyNowCommand Request { get; }
        public BuyNowCommandHandler Handler { get; }
        public Dictionary<Guid, BuyNowPurchase> Purchases { get; } = [];
        public List<string> Writes { get; } = [];
        public BuyNowSagaStarted? Published { get; private set; }
        public AuctionAuditData? PreviousAudit { get; private set; }
        public AuctionAuditData? CurrentAudit { get; private set; }
        public bool LockReleased { get; private set; }
        public bool CancelRead { get; init; }
        public string? FailureStage { get; init; }

        public PurchaseFixture(CancellationToken token = default)
        {
            Request = new BuyNowCommand(Auction.Id, Guid.NewGuid(), "buyer", Guid.NewGuid());
            var repository = InterfaceStub.For<IAuctionWriteRepository>((method, args) =>
            {
                Assert.Equal(token, args.OfType<CancellationToken>().Single());
                if (method.Name == nameof(IAuctionWriteRepository.GetByIdForUpdateAsync))
                {
                    return CancelRead ? Task.FromCanceled<AuctionEntity?>(token) : Task.FromResult<AuctionEntity?>(Auction);
                }
                Assert.Equal(nameof(IAuctionWriteRepository.UpdateAsync), method.Name);
                Record("update");
                return Task.CompletedTask;
            });
            var purchases = InterfaceStub.For<IBuyNowPurchaseRepository>((method, args) =>
            {
                Assert.Equal(token, args.OfType<CancellationToken>().Single());
                if (method.Name == nameof(IBuyNowPurchaseRepository.GetAsync))
                {
                    return Task.FromResult(Purchases.GetValueOrDefault((Guid)args[0]!));
                }
                Assert.Equal(nameof(IBuyNowPurchaseRepository.AddAsync), method.Name);
                var purchase = Assert.IsType<BuyNowPurchase>(args[0]);
                Purchases.Add(purchase.CorrelationId, purchase);
                Record("purchase");
                return Task.CompletedTask;
            });
            var publisher = InterfaceStub.For<IEventPublisher>((_, args) =>
            {
                Assert.Equal(token, args.OfType<CancellationToken>().Single());
                Published = Assert.IsType<BuyNowSagaStarted>(args[0]);
                Record("publish");
                return Task.CompletedTask;
            });
            var audit = InterfaceStub.For<IAuditPublisher>((_, args) =>
            {
                Assert.Equal(token, args.OfType<CancellationToken>().Single());
                CurrentAudit = Assert.IsType<AuctionAuditData>(args[1]);
                PreviousAudit = Assert.IsType<AuctionAuditData>(args[3]);
                Record("audit");
                return Task.CompletedTask;
            });
            var unit = InterfaceStub.For<IUnitOfWork>((method, args) =>
            {
                Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
                Assert.Equal(token, args.OfType<CancellationToken>().Single());
                Record("save");
                return Task.FromResult(1);
            });
            var distributedLock = InterfaceStub.For<IDistributedLock>((_, args) =>
            {
                Assert.Equal(token, args.OfType<CancellationToken>().Single());
                return Task.FromResult<IAsyncDisposable?>(new LockHandle(() => LockReleased = true));
            });
            Handler = new BuyNowCommandHandler(repository, NullLogger<BuyNowCommandHandler>.Instance,
                unit, distributedLock, audit, purchases, publisher);
        }

        public BuyNowPurchase ExistingPurchase() => new()
        {
            CorrelationId = Request.CorrelationId!.Value,
            AuctionId = Auction.Id,
            BuyerId = Request.BuyerId
        };

        private void Record(string stage)
        {
            Assert.False(LockReleased);
            Writes.Add(stage);
            if (stage == FailureStage) throw new InvalidOperationException("Dependency unavailable");
        }
    }

    private sealed class LockHandle(Action release) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            release();
            return ValueTask.CompletedTask;
        }
    }
}
