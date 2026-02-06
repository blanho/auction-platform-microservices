using MassTransit;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Sagas.BuyNow;

public class BuyNowSagaStateMachine : MassTransitStateMachine<BuyNowSagaState>
{
    public State ReservingAuction { get; private set; } = null!;
    public State CreatingOrder { get; private set; } = null!;
    public State CompletingAuction { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    public State Compensating { get; private set; } = null!;

    public Event<BuyNowSagaStarted> BuyNowStarted { get; private set; } = null!;
    public Event<AuctionReservedForBuyNow> AuctionReserved { get; private set; } = null!;
    public Event<AuctionReservationFailed> AuctionReservationFailed { get; private set; } = null!;
    public Event<BuyNowOrderCreated> OrderCreated { get; private set; } = null!;
    public Event<BuyNowOrderCreationFailed> OrderCreationFailed { get; private set; } = null!;
    public Event<BuyNowAuctionCompleted> AuctionCompleted { get; private set; } = null!;
    public Event<AuctionReservationReleased> ReservationReleased { get; private set; } = null!;

    public Schedule<BuyNowSagaState, BuyNowSagaTimedOut> SagaTimeout { get; private set; } = null!;

    public BuyNowSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => BuyNowStarted, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => AuctionReserved, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => AuctionReservationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCreated, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => OrderCreationFailed, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => AuctionCompleted, e => e.CorrelateById(m => m.Message.CorrelationId));
        Event(() => ReservationReleased, e => e.CorrelateById(m => m.Message.CorrelationId));

        Schedule(() => SagaTimeout, instance => instance.TimeoutTokenId, s =>
        {
            s.Delay = TimeSpan.FromMinutes(5);
            s.Received = r => r.CorrelateById(m => m.Message.CorrelationId);
        });

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
                })
                .Schedule(SagaTimeout, context => new BuyNowSagaTimedOut
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    BuyerId = context.Saga.BuyerId,
                    BuyerUsername = context.Saga.BuyerUsername,
                    TimedOutAt = DateTimeOffset.UtcNow.AddMinutes(5)
                })
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
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.CompletedAt = DateTimeOffset.UtcNow;
                })
                .Publish(context => new BuyNowSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = Guid.Empty,
                    Success = false,
                    FailureReason = context.Saga.FailureReason,
                    CompletedAt = context.Saga.CompletedAt ?? DateTimeOffset.UtcNow
                })
                .TransitionTo(Failed)
                .Finalize()
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
                .TransitionTo(Compensating)
        );

        During(CompletingAuction,
            When(AuctionCompleted)
                .Then(context =>
                {
                    context.Saga.CompletedAt = context.Message.CompletedAt;
                })
                .Unschedule(SagaTimeout)
                .Publish(context => new BuyNowSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = context.Saga.OrderId ?? Guid.Empty,
                    Success = true,
                    FailureReason = null,
                    CompletedAt = context.Saga.CompletedAt ?? DateTimeOffset.UtcNow
                })
                .TransitionTo(Completed)
                .Finalize()
        );

        During(Compensating,
            When(ReservationReleased)
                .Then(context =>
                {
                    context.Saga.CompletedAt = DateTimeOffset.UtcNow;
                })
                .Unschedule(SagaTimeout)
                .Publish(context => new BuyNowSagaCompleted
                {
                    CorrelationId = context.Saga.CorrelationId,
                    AuctionId = context.Saga.AuctionId,
                    OrderId = Guid.Empty,
                    Success = false,
                    FailureReason = context.Saga.FailureReason ?? "Transaction rolled back",
                    CompletedAt = context.Saga.CompletedAt ?? DateTimeOffset.UtcNow
                })
                .TransitionTo(Failed)
                .Finalize()
        );

        DuringAny(
            When(SagaTimeout.Received)
                .Then(context =>
                {
                    context.Saga.FailureReason = "Transaction timed out after 5 minutes";
                    context.Saga.CompletedAt = DateTimeOffset.UtcNow;
                })
                .IfElse(
                    context => context.Saga.CurrentState == nameof(ReservingAuction),
                    binder => binder
                        .Publish(context => new BuyNowSagaCompleted
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            AuctionId = context.Saga.AuctionId,
                            OrderId = Guid.Empty,
                            Success = false,
                            FailureReason = context.Saga.FailureReason,
                            CompletedAt = context.Saga.CompletedAt ?? DateTimeOffset.UtcNow
                        })
                        .TransitionTo(Failed)
                        .Finalize(),
                    binder => binder
                        .Publish(context => new ReleaseAuctionReservation
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            AuctionId = context.Saga.AuctionId,
                            Reason = "Transaction timed out"
                        })
                        .TransitionTo(Compensating)
                )
        );

        SetCompletedWhenFinalized();
    }
}
