using MassTransit;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Sagas.AuctionCompletion;

public class AuctionCompletionSagaStateMachine : MassTransitStateMachine<AuctionCompletionSagaState>
{
    public State CreatingOrder { get; private set; } = null!;
    public State SendingNotifications { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State ManualInterventionRequired { get; private set; } = null!;

    public Event<AuctionCompletionSagaStarted> SagaStarted { get; private set; } = null!;
    public Event<AuctionWinnerOrderCreated> OrderCreated { get; private set; } = null!;
    public Event<AuctionWinnerOrderFailed> OrderFailed { get; private set; } = null!;
    public Event<AuctionCompletionNotificationsSent> NotificationsSent { get; private set; } = null!;
    public Event<AuctionCompletionNotificationsFailed> NotificationsFailed { get; private set; } = null!;

    public Event<AuctionCompletionSagaTimedOut> TimedOut { get; private set; } = null!;
    public Event<RetryAuctionCompletionSaga> RetryRequested { get; private set; } = null!;

    public AuctionCompletionSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SagaStarted, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCreated, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => NotificationsSent, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => NotificationsFailed, e => e.CorrelateById(m => m.Message.CorrelationId));

        Event(() => TimedOut, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => RetryRequested, e => e.CorrelateById(m => m.Message.CorrelationId));

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
                .Then(context => context.Saga.SetTimeout(SagaConstants.AuctionCompletionTimeout))
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

        During(CreatingOrder, ManualInterventionRequired,
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
                .Then(context => context.Saga.ClearTimeout())
                .Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout))
                .TransitionTo(SendingNotifications),

            When(OrderFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .Then(context => context.Saga.ClearTimeout())
                .Publish(context => new SagaRecoveryRequired
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Workflow = "AuctionCompletion",
                    Step = "CreateOrder",
                    Reason = context.Saga.FailureReason!
                })
                .TransitionTo(ManualInterventionRequired)
        );

        During(SendingNotifications,
            When(NotificationsSent)
                .Then(context =>
                {
                    context.Saga.NotificationsSentAt = context.Message.SentAt;
                    context.Saga.CompletedAt = DateTimeOffset.UtcNow;
                })
                .Then(context => context.Saga.ClearTimeout())
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
                .Then(context => context.Saga.ClearTimeout())
                .Publish(context => new SagaRecoveryRequired
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Workflow = "AuctionCompletion",
                    Step = "Notifications",
                    Reason = "Notification delivery requires follow-up"
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

        During(CreatingOrder,
            When(TimedOut, context => context.Message.TimeoutTokenId == context.Saga.TimeoutTokenId)
                .Publish(context => new SagaRecoveryRequired
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Workflow = "AuctionCompletion",
                    Step = "CreateOrder",
                    Reason = "Order creation outcome is unknown; reconcile or retry the same command"
                })
                .TransitionTo(ManualInterventionRequired)
        );

        During(SendingNotifications,
            When(TimedOut, context => context.Message.TimeoutTokenId == context.Saga.TimeoutTokenId)
                .Then(context => context.Saga.ClearTimeout())
                .Publish(context => new SagaRecoveryRequired
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Workflow = "AuctionCompletion",
                    Step = "Notifications",
                    Reason = "Notification delivery requires follow-up"
                })
                .Publish(context => new AuctionCompletionSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Saga.OrderId,
                    Success = true,
                    FailureReason = "Notification acknowledgement timed out; delivery requires follow-up",
                    CompletedAt = DateTimeOffset.UtcNow
                })
                .Finalize(),
            Ignore(OrderCreated), Ignore(OrderFailed));

        During(CreatingOrder, SendingNotifications, ManualInterventionRequired, Ignore(SagaStarted));
        During(ManualInterventionRequired, Ignore(TimedOut));

        During(ManualInterventionRequired,
            When(RetryRequested)
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
                .Then(context => context.Saga.SetTimeout(SagaConstants.AuctionCompletionTimeout))
                .TransitionTo(CreatingOrder));
        During(Final, Ignore(SagaStarted), Ignore(OrderCreated), Ignore(OrderFailed), Ignore(NotificationsSent),
            Ignore(NotificationsFailed), Ignore(TimedOut), Ignore(RetryRequested));

        foreach (var state in new[] { CreatingOrder, SendingNotifications })
            During(state, Ignore(RetryRequested));

    }
}
