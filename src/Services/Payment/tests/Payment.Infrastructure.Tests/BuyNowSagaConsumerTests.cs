using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using OrchestrationService.Contracts.Events;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Messaging.Consumers;
using Payment.Infrastructure.Persistence;
using Xunit;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Payment.Infrastructure.Tests;

public class BuyNowSagaConsumerTests
{
    [Fact]
    public async Task CancellationBeforeCreationPreventsLateOrder()
    {
        var fixture = new Fixture(); var command = fixture.Command();
        await fixture.Cancel(new CancelBuyNowOrder { CorrelationId = command.CorrelationId, AuctionId = command.AuctionId, BuyerId = command.BuyerId });
        await fixture.Create(command);
        Assert.Empty(fixture.Orders);
        Assert.Single(fixture.Published.OfType<BuyNowOrderCancelled>());
        Assert.Single(fixture.Published.OfType<BuyNowOrderCreationFailed>());
    }

    [Fact]
    public async Task DuplicateCreationAndCancellationAreIdempotent()
    {
        var fixture = new Fixture(); var command = fixture.Command();
        await fixture.Create(command); await fixture.Create(command);
        var order = Assert.Single(fixture.Orders);
        Assert.True(order.AwaitingBuyNowCompletion);
        Assert.Equal(2, fixture.Published.OfType<BuyNowOrderCreated>().Count());
        var cancel = new CancelBuyNowOrder { CorrelationId = command.CorrelationId, AuctionId = command.AuctionId, BuyerId = command.BuyerId };
        await fixture.Cancel(cancel); await fixture.Cancel(cancel); await fixture.Create(command);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Single(fixture.Orders);
        Assert.Equal(2, fixture.Published.OfType<BuyNowOrderCancelled>().Count());
        Assert.Single(fixture.Published.OfType<BuyNowOrderCreationFailed>());
    }

    [Fact]
    public async Task AnotherAttemptCannotReuseOrCancelTheOwnersOrder()
    {
        var fixture = new Fixture(); var command = fixture.Command(); await fixture.Create(command);
        var other = command with { CorrelationId = Guid.NewGuid(), BuyerId = Guid.NewGuid() };
        await fixture.Create(other);
        await fixture.Cancel(new CancelBuyNowOrder { CorrelationId = other.CorrelationId, AuctionId = other.AuctionId, BuyerId = other.BuyerId });
        Assert.Equal(OrderStatus.PaymentPending, Assert.Single(fixture.Orders).Status);
        Assert.Single(fixture.Published.OfType<BuyNowOrderCreationFailed>());
    }

    [Fact]
    public async Task CancelledPurchaseAllowsANewBuyer()
    {
        var fixture = new Fixture(); var command = fixture.Command(); await fixture.Create(command);
        await fixture.Cancel(new CancelBuyNowOrder { CorrelationId = command.CorrelationId, AuctionId = command.AuctionId, BuyerId = command.BuyerId });
        await fixture.Create(command with { CorrelationId = Guid.NewGuid(), BuyerId = Guid.NewGuid() });
        Assert.Equal(2, fixture.Orders.Count);
        Assert.Single(fixture.Orders, x => x.Status == OrderStatus.PaymentPending);
    }

    [Fact]
    public async Task TransientStoreFailureEscapesForTransportRetry()
    {
        var fixture = new Fixture { StoreError = new TimeoutException("database unavailable") };
        await Assert.ThrowsAsync<TimeoutException>(() => fixture.Create(fixture.Command()));
        Assert.Empty(fixture.Published);
    }

    [Fact]
    public async Task WinnerOrderConsumerReplaysSameOrderAndRejectsADifferentWinner()
    {
        var fixture = new Fixture();
        var message = new CreateAuctionWinnerOrder
        {
            CorrelationId = Guid.NewGuid(),
            AuctionId = Guid.NewGuid(),
            WinnerId = Guid.NewGuid(),
            WinnerUsername = "winner",
            SellerId = Guid.NewGuid(),
            SellerUsername = "seller",
            ItemTitle = "item",
            Amount = 100
        };
        await fixture.CreateWinner(message); await fixture.CreateWinner(message);
        await fixture.CreateWinner(message with { WinnerId = Guid.NewGuid() });
        var order = Assert.Single(fixture.Orders);
        Assert.False(order.AwaitingBuyNowCompletion);
        Assert.Equal(2, fixture.Published.OfType<AuctionWinnerOrderCreated>().Count());
        Assert.Single(fixture.Published.OfType<AuctionWinnerOrderFailed>());
    }

    private sealed class Fixture : IBuyNowOrderAttemptStore
    {
        public List<Order> Orders { get; } = [];
        public List<object> Published { get; } = [];
        public Exception? StoreError { get; init; }
        private readonly Dictionary<Guid, BuyNowOrderAttempt> _attempts = [];
        private readonly IOrderRepository _orders;
        private readonly IUnitOfWork _unitOfWork;

        public Fixture()
        {
            _orders = Stub<IOrderRepository>((method, args) => method.Name switch
            {
                nameof(IOrderRepository.GetByAuctionIdAsync) => (object)Task.FromResult(Orders
                    .Where(x => x.AuctionId == (Guid)args![0]!).OrderBy(x => x.Status == OrderStatus.Cancelled).FirstOrDefault()),
                nameof(IOrderRepository.AddAsync) => Add((Order)args![0]!),
                nameof(IOrderRepository.UpdateAsync) => Task.FromResult((Order)args![0]!),
                _ => throw new NotSupportedException(method.Name)
            });
            _unitOfWork = Stub<IUnitOfWork>((method, _) => method.Name == nameof(IUnitOfWork.SaveChangesAsync)
                ? Task.FromResult(1) : throw new NotSupportedException(method.Name));
        }
        private Task<Order> Add(Order order) { Orders.Add(order); return Task.FromResult(order); }
        public Task<BuyNowOrderAttempt> GetAsync(Guid id, Guid auction, Guid buyer, CancellationToken ct)
        {
            if (StoreError is not null) throw StoreError;
            if (!_attempts.TryGetValue(id, out var attempt))
                _attempts[id] = attempt = new BuyNowOrderAttempt { CorrelationId = id, AuctionId = auction, BuyerId = buyer };
            return Task.FromResult(attempt);
        }
        public CreateBuyNowOrder Command() => new()
        {
            CorrelationId = Guid.NewGuid(),
            AuctionId = Guid.NewGuid(),
            BuyerId = Guid.NewGuid(),
            BuyerUsername = "buyer",
            SellerId = Guid.NewGuid(),
            SellerUsername = "seller",
            ItemTitle = "item",
            BuyNowPrice = 100
        };
        public Task Create(CreateBuyNowOrder m) => new CreateBuyNowOrderConsumer(_orders, this, _unitOfWork,
            NullLogger<CreateBuyNowOrderConsumer>.Instance).Consume(Context(m));
        public Task CreateWinner(CreateAuctionWinnerOrder m) => new CreateAuctionWinnerOrderConsumer(_orders, _unitOfWork).Consume(Context(m));
        public Task Cancel(CancelBuyNowOrder m) => new CancelBuyNowOrderConsumer(_orders, _unitOfWork, this).Consume(Context(m));
        private ConsumeContext<T> Context<T>(T message) where T : class => Stub<ConsumeContext<T>>((method, args) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => Publish(args![0]!),
            _ => throw new NotSupportedException(method.Name)
        });
        private Task Publish(object message) { Published.Add(message); return Task.CompletedTask; }
    }
    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, Proxy>(); ((Proxy)(object)proxy).Handler = handler; return proxy;
    }
    public class Proxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
