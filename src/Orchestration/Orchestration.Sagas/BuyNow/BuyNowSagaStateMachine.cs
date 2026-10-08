using MassTransit;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Sagas.BuyNow;

public class BuyNowSagaStateMachine : MassTransitStateMachine<BuyNowSagaState>
{
    public State ReservingAuction { get; private set; } = null!;
    public State CreatingOrder { get; private set; } = null!;
    public State CompletingAuction { get; private set; } = null!;
    public State ConfirmingOrder { get; private set; } = null!;
    public Event<BuyNowOrderConfirmed> OrderConfirmed { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State CancellingOrder { get; private set; } = null!;
    public State ManualInterventionRequired { get; private set; } = null!;
    public Event<BuyNowOrderCancelled> OrderCancelled { get; private set; } = null!;
    public Event<BuyNowCompensationFailed> CompensationFailed { get; private set; } = null!;
    public State Compensating { get; private set; } = null!;

    public Event<BuyNowSagaStarted> BuyNowStarted { get; private set; } = null!;
    public Event<AuctionReservedForBuyNow> AuctionReserved { get; private set; } = null!;
    public Event<AuctionReservationFailed> AuctionReservationFailed { get; private set; } = null!;
    public Event<BuyNowOrderCreated> OrderCreated { get; private set; } = null!;
    public Event<BuyNowOrderCreationFailed> OrderCreationFailed { get; private set; } = null!;
    public Event<BuyNowAuctionCompleted> AuctionCompleted { get; private set; } = null!;
    public Event<BuyNowAuctionCompletionFailed> AuctionCompletionFailed { get; private set; } = null!;
    public Event<AuctionReservationReleased> ReservationReleased { get; private set; } = null!;

    public Event<BuyNowSagaTimedOut> TimedOut { get; private set; } = null!;
    public Event<RetryBuyNowSaga> RetryRequested { get; private set; } = null!;

    public BuyNowSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderConfirmed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCancelled, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CompensationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => BuyNowStarted, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => AuctionReserved, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => AuctionReservationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCreated, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCreationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => AuctionCompleted, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => AuctionCompletionFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ReservationReleased, e => e.CorrelateById(m => m.Message.CorrelationId));

        Event(() => TimedOut, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => RetryRequested, e => e.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            When(BuyNowStarted)
                .Then(context =>
                {
                    context.Saga.AuctionId = context.Message.AuctionId;
                    context.Saga.BuyerId = context.Message.BuyerId;
                    context.Saga.BuyerUsername = context.Message.BuyerUsername;
                    context.Saga.SellerId = context.Message.SellerId;
                    context.Saga.SellerUsername = context.Message.SellerUsername;
                    context.Saga.BuyNowPrice = context.Message.BuyNowPrice;
                    context.Saga.ItemTitle = context.Message.ItemTitle;
                    context.Saga.StartedAt = context.Message.StartedAt;
                    context.Saga.RecoveryStep = "ReleaseReservation";
                })
                .Then(context => context.Saga.SetTimeout(SagaConstants.BuyNowTimeout))
                .Publish(context => new ReserveAuctionForBuyNow
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    BuyerId = context.Saga.BuyerId,
                    BuyerUsername = context.Saga.BuyerUsername
                })
                .TransitionTo(ReservingAuction)
        );

        During(ReservingAuction,
            When(AuctionReserved)
                .Then(context =>
                {
                    context.Saga.SellerId = context.Message.SellerId;
                    context.Saga.SellerUsername = context.Message.SellerUsername;
                    context.Saga.BuyNowPrice = context.Message.BuyNowPrice;
                    context.Saga.ItemTitle = context.Message.ItemTitle;
                })
                .Publish(context => new CreateBuyNowOrder
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    BuyerId = context.Saga.BuyerId,
                    BuyerUsername = context.Saga.BuyerUsername,
                    SellerId = context.Saga.SellerId,
                    SellerUsername = context.Saga.SellerUsername,
                    BuyNowPrice = context.Saga.BuyNowPrice,
                    ItemTitle = context.Saga.ItemTitle
                })
                .TransitionTo(CreatingOrder),

            When(AuctionReservationFailed)
                .Then(context => context.Saga.FailureReason = context.Message.Reason)
                .Publish(context => new ReleaseAuctionReservation
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Reason = context.Saga.FailureReason!
                })
                .Then(context => context.Saga.ClearTimeout())
                .Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout))
                .TransitionTo(Compensating)
        );

        During(CreatingOrder,
            When(OrderCreated)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                })
                .Publish(context => new CompleteBuyNowAuction
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Saga.OrderId ?? Guid.Empty,
                    BuyerId = context.Saga.BuyerId,
                    BuyerUsername = context.Saga.BuyerUsername
                })
                .TransitionTo(CompletingAuction),

            When(OrderCreationFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .Publish(context => new ReleaseAuctionReservation
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Reason = "Order creation failed"
                })
                .Then(context => context.Saga.ClearTimeout())
                .Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout))
                .TransitionTo(Compensating)
        );

        During(CompletingAuction, Compensating, ManualInterventionRequired,
            When(AuctionCompleted)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.RecoveryStep = "ConfirmOrder";
                    context.Saga.RetryCount = 0;
                    context.Saga.SetTimeout(SagaConstants.StepTimeout);
                })
                .Publish(context => new ConfirmBuyNowOrder
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Saga.OrderId!.Value
                })
                .TransitionTo(ConfirmingOrder)
        );

        During(ConfirmingOrder, ManualInterventionRequired,
            When(OrderConfirmed)
                .Then(context => { context.Saga.ClearTimeout(); context.Saga.CompletedAt = DateTimeOffset.UtcNow; })
                .Publish(context => new BuyNowSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Message.OrderId,
                    Success = true,
                    CompletedAt = context.Saga.CompletedAt!.Value
                })
                .Finalize()
        );

        During(ConfirmingOrder,
            Ignore(AuctionCompleted), Ignore(OrderCreated), Ignore(AuctionReserved), Ignore(BuyNowStarted),
            When(TimedOut, context => context.Message.TimeoutTokenId == context.Saga.TimeoutTokenId)
                .Then(context => context.Saga.RetryCount++)
                .IfElse(context => context.Saga.RetryCount >= SagaConstants.MaxStepRetries,
                    binder => binder
                        .Then(context => context.Saga.ClearTimeout())
                        .Publish(context => new SagaRecoveryRequired
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            AuctionId = context.Saga.AuctionId,
                            Workflow = "BuyNow",
                            Step = "ConfirmOrder",
                            Reason = "Order activation acknowledgement is missing"
                        }).TransitionTo(ManualInterventionRequired),
                    binder => binder
                        .Publish(context => new ConfirmBuyNowOrder
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            AuctionId = context.Saga.AuctionId,
                            OrderId = context.Saga.OrderId!.Value
                        }).Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout)))
        );

        During(CompletingAuction,
            When(AuctionCompletionFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                })
                .Publish(context => new ReleaseAuctionReservation
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Reason = $"Auction completion failed: {context.Message.Reason}"
                })
                .Then(context => context.Saga.ClearTimeout())
                .Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout))
                .TransitionTo(Compensating)
        );

        During(Compensating, ManualInterventionRequired,
            When(ReservationReleased)
                .Publish(context => new CancelBuyNowOrder
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    BuyerId = context.Saga.BuyerId,
                    Reason = context.Saga.FailureReason ?? "Purchase cancelled"
                })
                .Then(context => { context.Saga.RetryCount = 0; context.Saga.RecoveryStep = "CancelOrder"; })
                .Then(context => context.Saga.ClearTimeout())
                .Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout))
                .TransitionTo(CancellingOrder)
        );

        During(CancellingOrder, ManualInterventionRequired,
            When(OrderCancelled)
                .Then(context => context.Saga.ClearTimeout())
                .Publish(context => new BuyNowSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Message.OrderId ?? Guid.Empty,
                    Success = false,
                    FailureReason = context.Saga.FailureReason ?? "Purchase cancelled",
                    CompletedAt = DateTimeOffset.UtcNow
                })
                .Finalize()
        );

        During(ReservingAuction, CreatingOrder, CompletingAuction,
            When(TimedOut, context => context.Message.TimeoutTokenId == context.Saga.TimeoutTokenId)
                .Then(context => context.Saga.FailureReason = "Purchase timed out; compensation pending")
                .Publish(context => new ReleaseAuctionReservation
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Reason = context.Saga.FailureReason!
                })
                .Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout))
                .TransitionTo(Compensating)
        );

        During(Compensating, CancellingOrder,
            When(TimedOut, context => context.Message.TimeoutTokenId == context.Saga.TimeoutTokenId)
                .Then(context => context.Saga.RetryCount++)
                .IfElse(context => context.Saga.RetryCount >= SagaConstants.MaxCompensationRetries,
                    binder => binder
                        .Publish(context => new SagaRecoveryRequired
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            AuctionId = context.Saga.AuctionId,
                            Workflow = "BuyNow",
                            Step = context.Saga.RecoveryStep,
                            Reason = "Compensation acknowledgements were not received"
                        })
                        .TransitionTo(ManualInterventionRequired),
                    binder => binder
                        .IfElse(context => context.Saga.CurrentState == nameof(Compensating),
                            release => release.Publish(context => new ReleaseAuctionReservation
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                AuctionId = context.Saga.AuctionId,
                                Reason = context.Saga.FailureReason ?? "Purchase cancelled"
                            }),
                            cancel => cancel.Publish(context => new CancelBuyNowOrder
                            {
                                CorrelationId = context.Saga.CorrelationId,
                                AuctionId = context.Saga.AuctionId,
                                BuyerId = context.Saga.BuyerId,
                                Reason = context.Saga.FailureReason ?? "Purchase cancelled"
                            }))
                        .Then(context => context.Saga.SetTimeout(SagaConstants.StepTimeout))),
            When(CompensationFailed)
                .Then(context => context.Saga.ClearTimeout())
                .Publish(context => new SagaRecoveryRequired
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    Workflow = "BuyNow",
                    Step = context.Saga.RecoveryStep,
                    Reason = context.Message.Reason
                })
                .TransitionTo(ManualInterventionRequired)
        );

        // Late step replies cannot restart forward progress after compensation began.
        During(Compensating, CancellingOrder, ManualInterventionRequired,
            When(OrderCreated).Then(context => context.Saga.OrderId = context.Message.OrderId),
            Ignore(AuctionReserved), Ignore(AuctionReservationFailed),
            Ignore(OrderCreationFailed), Ignore(AuctionCompletionFailed));
        foreach (var state in new[] { CreatingOrder, CompletingAuction, Compensating, CancellingOrder, ManualInterventionRequired })
            During(state, Ignore(BuyNowStarted));
        During(ReservingAuction, Ignore(BuyNowStarted));
        During(CompletingAuction, Ignore(OrderCreated), Ignore(AuctionReserved));
        During(CreatingOrder, Ignore(AuctionReserved));
        During(CancellingOrder, Ignore(ReservationReleased));
        During(ManualInterventionRequired, Ignore(TimedOut));


        During(ManualInterventionRequired,
            When(RetryRequested)
                .Then(context => { context.Saga.RetryCount = 0; context.Saga.SetTimeout(SagaConstants.StepTimeout); })
                .IfElse(context => context.Saga.RecoveryStep == "ConfirmOrder",
                    confirm => confirm.Publish(context => new ConfirmBuyNowOrder
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        AuctionId = context.Saga.AuctionId,
                        OrderId = context.Saga.OrderId!.Value
                    }).TransitionTo(ConfirmingOrder),
                    rollback => rollback.IfElse(context => context.Saga.RecoveryStep == "CancelOrder",
                        cancel => cancel.Publish(context => new CancelBuyNowOrder
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            AuctionId = context.Saga.AuctionId,
                            BuyerId = context.Saga.BuyerId,
                            Reason = context.Saga.FailureReason ?? "Purchase cancelled"
                        }).TransitionTo(CancellingOrder),
                        release => release.Publish(context => new ReleaseAuctionReservation
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            AuctionId = context.Saga.AuctionId,
                            Reason = context.Saga.FailureReason ?? "Purchase cancelled"
                        }).TransitionTo(Compensating)))
        );
        During(Final, Ignore(BuyNowStarted), Ignore(AuctionReserved), Ignore(AuctionReservationFailed),
            Ignore(OrderCreated), Ignore(OrderCreationFailed), Ignore(AuctionCompleted), Ignore(AuctionCompletionFailed),
            Ignore(ReservationReleased), Ignore(OrderCancelled), Ignore(OrderConfirmed), Ignore(CompensationFailed),
            Ignore(TimedOut), Ignore(RetryRequested));

        foreach (var state in new[] { ReservingAuction, CreatingOrder, CompletingAuction, Compensating, CancellingOrder, ConfirmingOrder })
            During(state, Ignore(RetryRequested));

    }
}
