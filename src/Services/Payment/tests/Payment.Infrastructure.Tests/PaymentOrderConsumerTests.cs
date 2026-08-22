using System.Reflection;
using AuctionService.Contracts.Events;
using BuildingBlocks.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Filtering;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Infrastructure.Messaging.Consumers;
using Xunit;

namespace Payment.Infrastructure.Tests;

public sealed class PaymentOrderConsumerTests
{
    [Fact]
    public async Task BuyNowExecuted_CreatesOrderAndCommits()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var consumer = new BuyNowExecutedConsumer(
            repository,
            unitOfWork,
            NullLogger<BuyNowExecutedConsumer>.Instance);
        var cancellationToken = new CancellationTokenSource().Token;
        var message = CreateBuyNowExecutedEvent();

        await consumer.Consume(ConsumeContextFactory.Create(message, cancellationToken));

        var order = Assert.Single(repository.AddedOrders);
        Assert.Equal(message.AuctionId, order.AuctionId);
        Assert.Equal(message.BuyerId, order.BuyerId);
        Assert.Equal(message.Buyer, order.BuyerUsername);
        Assert.Equal(message.SellerId, order.SellerId);
        Assert.Equal(message.Seller, order.SellerUsername);
        Assert.Equal(message.ItemTitle, order.ItemTitle);
        Assert.Equal(message.BuyNowPrice, order.WinningBid);
        Assert.Equal(1, unitOfWork.SaveCalls);
        Assert.Equal(cancellationToken, unitOfWork.LastCancellationToken);
    }

    [Fact]
    public async Task BuyNowExecuted_WhenOrderAlreadyExists_DoesNotCreateDuplicate()
    {
        var repository = new FakeOrderRepository { ExistingOrder = CreateOrder() };
        var unitOfWork = new FakeUnitOfWork();
        var consumer = new BuyNowExecutedConsumer(
            repository,
            unitOfWork,
            NullLogger<BuyNowExecutedConsumer>.Instance);

        await consumer.Consume(ConsumeContextFactory.Create(CreateBuyNowExecutedEvent()));

        Assert.Empty(repository.AddedOrders);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task AuctionFinished_WhenItemWasSold_CreatesOrderAndCommits()
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var consumer = new AuctionFinishedConsumer(
            repository,
            unitOfWork,
            NullLogger<AuctionFinishedConsumer>.Instance);
        var cancellationToken = new CancellationTokenSource().Token;
        var message = CreateAuctionFinishedEvent();

        await consumer.Consume(ConsumeContextFactory.Create(message, cancellationToken));

        var order = Assert.Single(repository.AddedOrders);
        Assert.Equal(message.AuctionId, order.AuctionId);
        Assert.Equal(message.WinnerId, order.BuyerId);
        Assert.Equal(message.WinnerUsername, order.BuyerUsername);
        Assert.Equal(message.SellerId, order.SellerId);
        Assert.Equal(message.SellerUsername, order.SellerUsername);
        Assert.Equal(message.ItemTitle, order.ItemTitle);
        Assert.Equal(message.SoldAmount, order.WinningBid);
        Assert.Equal(1, unitOfWork.SaveCalls);
        Assert.Equal(cancellationToken, unitOfWork.LastCancellationToken);
    }

    [Theory]
    [InlineData(false, "winner")]
    [InlineData(true, null)]
    [InlineData(true, "")]
    public async Task AuctionFinished_WhenAuctionHasNoValidSale_SkipsOrderCreation(
        bool itemSold,
        string? winnerUsername)
    {
        var repository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var consumer = new AuctionFinishedConsumer(
            repository,
            unitOfWork,
            NullLogger<AuctionFinishedConsumer>.Instance);
        var message = CreateAuctionFinishedEvent() with
        {
            ItemSold = itemSold,
            WinnerUsername = winnerUsername
        };

        await consumer.Consume(ConsumeContextFactory.Create(message));

        Assert.Equal(0, repository.GetByAuctionIdCalls);
        Assert.Empty(repository.AddedOrders);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task AuctionFinished_WhenOrderAlreadyExists_DoesNotCreateDuplicate()
    {
        var repository = new FakeOrderRepository { ExistingOrder = CreateOrder() };
        var unitOfWork = new FakeUnitOfWork();
        var consumer = new AuctionFinishedConsumer(
            repository,
            unitOfWork,
            NullLogger<AuctionFinishedConsumer>.Instance);

        await consumer.Consume(ConsumeContextFactory.Create(CreateAuctionFinishedEvent()));

        Assert.Empty(repository.AddedOrders);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    private static BuyNowExecutedEvent CreateBuyNowExecutedEvent() => new()
    {
        AuctionId = Guid.NewGuid(),
        BuyerId = Guid.NewGuid(),
        Buyer = "buy-now-buyer",
        SellerId = Guid.NewGuid(),
        Seller = "seller",
        BuyNowPrice = 250m,
        ItemTitle = "Buy now item",
        ExecutedAt = DateTimeOffset.UtcNow
    };

    private static AuctionFinishedEvent CreateAuctionFinishedEvent() => new()
    {
        ItemSold = true,
        AuctionId = Guid.NewGuid(),
        WinnerId = Guid.NewGuid(),
        WinnerUsername = "winning-bidder",
        SellerId = Guid.NewGuid(),
        SellerUsername = "seller",
        SoldAmount = 175m,
        ItemTitle = "Auction item"
    };

    private static Order CreateOrder() => Order.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "buyer",
        Guid.NewGuid(),
        "seller",
        "Existing order",
        100m);

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public Order? ExistingOrder { get; init; }
        public int GetByAuctionIdCalls { get; private set; }
        public List<Order> AddedOrders { get; } = [];

        public Task<Order?> GetByAuctionIdAsync(Guid auctionId)
        {
            GetByAuctionIdCalls++;
            return Task.FromResult(ExistingOrder);
        }

        public Task<Order> AddAsync(Order order)
        {
            AddedOrders.Add(order);
            return Task.FromResult(order);
        }

        public Task<Order?> GetByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<PaginatedResult<Order>> GetByBuyerUsernameAsync(OrderQueryParams queryParams) => throw new NotSupportedException();
        public Task<PaginatedResult<Order>> GetBySellerUsernameAsync(OrderQueryParams queryParams) => throw new NotSupportedException();
        public Task<Order> UpdateAsync(Order order) => throw new NotSupportedException();
        public Task<Order> UpdateAsync(Order order, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> GetCountByBuyerUsernameAsync(string username) => throw new NotSupportedException();
        public Task<int> GetCountBySellerUsernameAsync(string username) => throw new NotSupportedException();
        public Task<PaginatedResult<Order>> GetAllAsync(OrderQueryParams queryParams, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<RevenueStatsDto> GetRevenueStatsAsync(DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<DailyRevenueStatDto>> GetDailyRevenueAsync(int days, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<TopSellerDto>> GetTopSellersAsync(int limit, string period, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<List<TopBuyerDto>> GetTopBuyersAsync(int limit, string period, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCalls { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            LastCancellationToken = cancellationToken;
            return Task.FromResult(1);
        }

        public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Dispose() { }
    }

    private static class ConsumeContextFactory
    {
        public static ConsumeContext<T> Create<T>(T message, CancellationToken cancellationToken = default)
            where T : class
        {
            var context = DispatchProxy.Create<ConsumeContext<T>, ConsumeContextProxy<T>>();
            var proxy = (ConsumeContextProxy<T>)(object)context;
            proxy.MessageValue = message;
            proxy.CancellationTokenValue = cancellationToken;
            return context;
        }
    }

    public class ConsumeContextProxy<T> : DispatchProxy
        where T : class
    {
        public T MessageValue { get; set; } = default!;
        public CancellationToken CancellationTokenValue { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod?.Name switch
        {
            "get_Message" => MessageValue,
            "get_CancellationToken" => CancellationTokenValue,
            _ => throw new NotSupportedException($"ConsumeContext member '{targetMethod?.Name}' is not used by these tests")
        };
    }
}
