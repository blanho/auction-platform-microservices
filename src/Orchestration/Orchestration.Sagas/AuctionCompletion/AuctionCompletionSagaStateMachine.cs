using MassTransit;
using Orchestration.Sagas.AuctionCompletion.Events;

namespace Orchestration.Sagas.AuctionCompletion;

public class AuctionCompletionSagaStateMachine : MassTransitStateMachine<AuctionCompletionSagaState>
{
    public State CreatingOrder { get; private set; } = null!;
    public State SendingNotifications { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    public State Compensating { get; private set; } = null!;

    public Event<AuctionCompletionSagaStarted> SagaStarted { get; private set; } = null!;
    public Event<AuctionWinnerOrderCreated> OrderCreated { get; private set; } = null!;
    public Event<AuctionWinnerOrderFailed> OrderFailed { get; private set; } = null!;
    public Event<AuctionCompletionNotificationsSent> NotificationsSent { get; private set; } = null!;
    public Event<AuctionCompletionNotificationsFailed> NotificationsFailed { get; private set; } = null!;
    public Event<AuctionCompletionReverted> CompletionReverted { get; private set; } = null!;

    public AuctionCompletionSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SagaStarted, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCreated, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => NotificationsSent, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => NotificationsFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CompletionReverted, e => e.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            When(SagaStarted)
                .Then(context =>
                {
                    context.Saga.AuctionId = context.Message.AuctionId;
                    context.Saga.SellerId = context.Message.SellerId;
                    context.Saga.SellerUsername = context.Message.SellerUsername;
                    context.Saga.WinnerId = context.Message.WinnerId;
                    context.Saga.WinnerUsername = context.Message.WinnerUsername;
                    context.Saga.WinningBidAmount = context.Message.WinningBidAmount;
                    context.Saga.ItemTitle = context.Message.ItemTitle;
                    context.Saga.StartedAt = context.Message.AuctionEndedAt;
                })
                .Publish(context => new CreateAuctionWinnerOrder
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    SellerId = context.Saga.SellerId,
                    SellerUsername = context.Saga.SellerUsername,
                    WinnerId = context.Saga.WinnerId,
                    WinnerUsername = context.Saga.WinnerUsername,
                    Amount = context.Saga.WinningBidAmount,
                    ItemTitle = context.Saga.ItemTitle
                })
                .TransitionTo(CreatingOrder)
        );

        During(CreatingOrder,
            When(OrderCreated)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.OrderCreatedAt = context.Message.CreatedAt;
                })
                .Publish(context => new SendAuctionCompletionNotifications
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Saga.OrderId ?? Guid.Empty,
                    SellerId = context.Saga.SellerId,
                    SellerUsername = context.Saga.SellerUsername,
                    WinnerId = context.Saga.WinnerId,
                    WinnerUsername = context.Saga.WinnerUsername,
                    Amount = context.Saga.WinningBidAmount,
                    ItemTitle = context.Saga.ItemTitle
                })
                .TransitionTo(SendingNotifications),

            When(OrderFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .Publish(context => new RevertAuctionCompletion
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Reason = "Order creation failed: " + context.Saga.FailureReason
                })
                .TransitionTo(Compensating)
        );

        During(SendingNotifications,
            When(NotificationsSent)
                .Then(context =>
                {
                    context.Saga.NotificationsSentAt = context.Message.SentAt;
                    context.Saga.CompletedAt = DateTimeOffset.UtcNow;
                })
                .Publish(context => new AuctionCompletionSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Saga.OrderId,
                    Success = true,
                    FailureReason = null,
                    CompletedAt = context.Saga.CompletedAt ?? DateTimeOffset.UtcNow
                })
                .TransitionTo(Completed)
                .Finalize(),

            When(NotificationsFailed)
                .Then(context =>
                {
                    context.Saga.CompletedAt = DateTimeOffset.UtcNow;
                })
                .Publish(context => new AuctionCompletionSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Saga.OrderId,
                    Success = true,
                    FailureReason = "Notifications partially failed: " + context.Message.Reason,
                    CompletedAt = context.Saga.CompletedAt ?? DateTimeOffset.UtcNow
                })
                .TransitionTo(Completed)
                .Finalize()
        );

        During(Compensating,
            When(CompletionReverted)
                .Then(context =>
                {
                    context.Saga.CompletedAt = context.Message.RevertedAt;
                })
                .Publish(context => new AuctionCompletionSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = null,
                    Success = false,
                    FailureReason = context.Saga.FailureReason ?? "Transaction rolled back",
                    CompletedAt = context.Saga.CompletedAt ?? DateTimeOffset.UtcNow
                })
                .TransitionTo(Failed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
