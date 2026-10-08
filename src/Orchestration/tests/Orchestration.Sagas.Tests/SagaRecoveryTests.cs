using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Orchestration.Sagas.BuyNow;
using Orchestration.Sagas.AuctionCompletion;
using OrchestrationService.Contracts.Events;
using Xunit;

namespace Orchestration.Sagas.Tests;

public class SagaRecoveryTests
{
    private static ServiceProvider Provider() => new ServiceCollection().AddMassTransitTestHarness(x =>
    {
        x.SetTestTimeouts(testTimeout: TimeSpan.FromSeconds(10), testInactivityTimeout: TimeSpan.FromSeconds(2));
        x.AddSagaStateMachine<BuyNowSagaStateMachine, BuyNowSagaState>().InMemoryRepository();
        x.AddSagaStateMachine<AuctionCompletionSagaStateMachine, AuctionCompletionSagaState>().InMemoryRepository();
    }).BuildServiceProvider(true);

    [Fact]
    public async Task StaleTimeoutCannotCancelAProgressedPurchase()
    {
        await using var provider = Provider();
        var harness = provider.GetRequiredService<ITestHarness>(); await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<BuyNowSagaStateMachine, BuyNowSagaState>();
        var id = Guid.NewGuid();
        await harness.Bus.Publish(new BuyNowSagaStarted { CorrelationId = id, AuctionId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.ReservingAuction));
        var oldToken = saga.Created.Contains(id)!.TimeoutTokenId;
        await harness.Bus.Publish(new AuctionReservedForBuyNow { CorrelationId = id });
        Assert.NotNull(await saga.Exists(id, x => x.CreatingOrder));
        await harness.Bus.Publish(new BuyNowOrderCreated { CorrelationId = id, OrderId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.CompletingAuction));
        await harness.Bus.Publish(new BuyNowAuctionCompleted { CorrelationId = id, OrderId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.ConfirmingOrder));
        var timeout = new BuyNowSagaTimedOut { CorrelationId = id, TimeoutTokenId = oldToken };
        await harness.Bus.Publish(timeout);
        Assert.True(await saga.Consumed.Any<BuyNowSagaTimedOut>(x => x.Context.Message.Timestamp == timeout.Timestamp));
        Assert.Equal("ConfirmingOrder", saga.Created.Contains(id)!.CurrentState);
        Assert.NotEqual(oldToken, saga.Created.Contains(id)!.TimeoutTokenId);
        Assert.False(harness.Published.Select<ReleaseAuctionReservation>().Any());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreatedOrderCompletesOrWaitsForConfirmedRollback(bool succeeds)
    {
        await using var provider = Provider();
        var harness = provider.GetRequiredService<ITestHarness>(); await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<BuyNowSagaStateMachine, BuyNowSagaState>();
        var id = Guid.NewGuid(); var auction = Guid.NewGuid(); var order = Guid.NewGuid();
        await harness.Bus.Publish(new BuyNowSagaStarted { CorrelationId = id, AuctionId = auction, BuyerId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.ReservingAuction));
        await harness.Bus.Publish(new AuctionReservedForBuyNow { CorrelationId = id, AuctionId = auction });
        Assert.NotNull(await saga.Exists(id, x => x.CreatingOrder));
        await harness.Bus.Publish(new BuyNowOrderCreated { CorrelationId = id, AuctionId = auction, OrderId = order });
        Assert.NotNull(await saga.Exists(id, x => x.CompletingAuction));
        if (succeeds)
        {
            await harness.Bus.Publish(new BuyNowAuctionCompleted { CorrelationId = id, AuctionId = auction, OrderId = order });
            Assert.NotNull(await saga.Exists(id, x => x.ConfirmingOrder));
            await harness.Bus.Publish(new BuyNowOrderConfirmed { CorrelationId = id, AuctionId = auction, OrderId = order });
        }
        else
        {
            await harness.Bus.Publish(new BuyNowAuctionCompletionFailed { CorrelationId = id, AuctionId = auction, Reason = "failed" });
            Assert.NotNull(await saga.Exists(id, x => x.Compensating));
            await harness.Bus.Publish(new AuctionReservationReleased { CorrelationId = id, AuctionId = auction });
            Assert.NotNull(await saga.Exists(id, x => x.CancellingOrder));
            await harness.Bus.Publish(new BuyNowOrderCancelled { CorrelationId = id, AuctionId = auction, OrderId = order });
        }
        Assert.True(await harness.Published.Any<BuyNowSagaCompleted>(x =>
            x.Context.Message.Success == succeeds && x.Context.Message.OrderId == order));
    }

    [Fact]
    public async Task ExhaustedCompensationStaysAvailableForLateAcknowledgement()
    {
        await using var provider = Provider();
        var harness = provider.GetRequiredService<ITestHarness>(); await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<BuyNowSagaStateMachine, BuyNowSagaState>();
        var id = Guid.NewGuid();
        await harness.Bus.Publish(new BuyNowSagaStarted { CorrelationId = id, AuctionId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.ReservingAuction));
        await harness.Bus.Publish(new BuyNowSagaTimedOut { TimeoutTokenId = saga.Created.Contains(id)!.TimeoutTokenId, CorrelationId = id });
        Assert.NotNull(await saga.Exists(id, x => x.Compensating));
        for (var i = 0; i < SagaConstants.MaxCompensationRetries; i++)
        {
            var timeout = new BuyNowSagaTimedOut { TimeoutTokenId = saga.Created.Contains(id)!.TimeoutTokenId, CorrelationId = id };
            await harness.Bus.Publish(timeout);
            Assert.True(await saga.Consumed.Any<BuyNowSagaTimedOut>(x => x.Context.Message.Timestamp == timeout.Timestamp));
        }
        Assert.NotNull(await saga.Exists(id, x => x.ManualInterventionRequired));
        Assert.True(await harness.Published.Any<SagaRecoveryRequired>());
        await harness.Bus.Publish(new AuctionReservationReleased { CorrelationId = id });
        Assert.NotNull(await saga.Exists(id, x => x.CancellingOrder));
        await harness.Bus.Publish(new BuyNowOrderCancelled { CorrelationId = id });
        Assert.True(await harness.Published.Any<BuyNowSagaCompleted>(x => !x.Context.Message.Success));
    }

    [Fact]
    public async Task ReservationTimeoutWaitsForReleaseAndOrderCancellation()
    {
        await using var provider = Provider();
        var harness = provider.GetRequiredService<ITestHarness>(); await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<BuyNowSagaStateMachine, BuyNowSagaState>();
        var id = Guid.NewGuid(); var auction = Guid.NewGuid();
        await harness.Bus.Publish(new BuyNowSagaStarted { CorrelationId = id, AuctionId = auction, BuyerId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.ReservingAuction));
        await harness.Bus.Publish(new BuyNowSagaTimedOut { TimeoutTokenId = saga.Created.Contains(id)!.TimeoutTokenId, CorrelationId = id, AuctionId = auction });
        Assert.NotNull(await saga.Exists(id, x => x.Compensating));
        Assert.True(await harness.Published.Any<ReleaseAuctionReservation>());
        // A delayed reserve reply must not resume order creation.
        await harness.Bus.Publish(new AuctionReservedForBuyNow { CorrelationId = id, AuctionId = auction });
        Assert.True(await saga.Consumed.Any<AuctionReservedForBuyNow>());
        await harness.Bus.Publish(new AuctionReservationReleased { CorrelationId = id, AuctionId = auction });
        Assert.NotNull(await saga.Exists(id, x => x.CancellingOrder));
        Assert.True(await harness.Published.Any<CancelBuyNowOrder>());
        await harness.Bus.Publish(new BuyNowOrderCancelled { CorrelationId = id, AuctionId = auction });
        Assert.True(await harness.Published.Any<BuyNowSagaCompleted>(x => !x.Context.Message.Success));
        Assert.Empty(harness.Published.Select<CreateBuyNowOrder>());
    }

    [Fact]
    public async Task CompletionWinningTheTimeoutRacePreservesOrder()
    {
        await using var provider = Provider();
        var harness = provider.GetRequiredService<ITestHarness>(); await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<BuyNowSagaStateMachine, BuyNowSagaState>();
        var id = Guid.NewGuid(); var order = Guid.NewGuid();
        await harness.Bus.Publish(new BuyNowSagaStarted { CorrelationId = id, AuctionId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.ReservingAuction));
        await harness.Bus.Publish(new BuyNowSagaTimedOut { TimeoutTokenId = saga.Created.Contains(id)!.TimeoutTokenId, CorrelationId = id });
        Assert.NotNull(await saga.Exists(id, x => x.Compensating));
        await harness.Bus.Publish(new BuyNowAuctionCompleted { CorrelationId = id, OrderId = order });
        Assert.NotNull(await saga.Exists(id, x => x.ConfirmingOrder));
        await harness.Bus.Publish(new BuyNowOrderConfirmed { CorrelationId = id, OrderId = order });
        Assert.True(await harness.Published.Any<BuyNowSagaCompleted>(x => x.Context.Message.Success && x.Context.Message.OrderId == order));
        Assert.Empty(harness.Published.Select<CancelBuyNowOrder>());
    }

    [Fact]
    public async Task NotificationTimeoutKeepsSuccessfulSaleAndOrderId()
    {
        await using var provider = Provider();
        var harness = provider.GetRequiredService<ITestHarness>(); await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<AuctionCompletionSagaStateMachine, AuctionCompletionSagaState>();
        var id = Guid.NewGuid(); var order = Guid.NewGuid();
        await harness.Bus.Publish(new AuctionCompletionSagaStarted { CorrelationId = id, AuctionId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.CreatingOrder));
        await harness.Bus.Publish(new AuctionWinnerOrderCreated { CorrelationId = id, OrderId = order });
        Assert.NotNull(await saga.Exists(id, x => x.SendingNotifications));
        await harness.Bus.Publish(new AuctionCompletionSagaTimedOut { TimeoutTokenId = saga.Created.Contains(id)!.TimeoutTokenId, CorrelationId = id });
        Assert.True(await harness.Published.Any<AuctionCompletionSagaCompleted>(x => x.Context.Message.Success && x.Context.Message.OrderId == order));
        Assert.Empty(harness.Published.Select<RevertAuctionCompletion>());
    }

    [Fact]
    public async Task UnknownOrderOutcomeIsRetainedAndLateSuccessCanRecover()
    {
        await using var provider = Provider();
        var harness = provider.GetRequiredService<ITestHarness>(); await harness.Start();
        var saga = harness.GetSagaStateMachineHarness<AuctionCompletionSagaStateMachine, AuctionCompletionSagaState>();
        var id = Guid.NewGuid();
        await harness.Bus.Publish(new AuctionCompletionSagaStarted { CorrelationId = id, AuctionId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.CreatingOrder));
        await harness.Bus.Publish(new AuctionCompletionSagaTimedOut { TimeoutTokenId = saga.Created.Contains(id)!.TimeoutTokenId, CorrelationId = id });
        Assert.NotNull(await saga.Exists(id, x => x.ManualInterventionRequired));
        Assert.True(await harness.Published.Any<SagaRecoveryRequired>());
        await harness.Bus.Publish(new AuctionWinnerOrderCreated { CorrelationId = id, OrderId = Guid.NewGuid() });
        Assert.NotNull(await saga.Exists(id, x => x.SendingNotifications));
    }
}
