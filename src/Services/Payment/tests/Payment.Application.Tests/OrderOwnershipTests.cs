using System.Reflection;
using Payment.Application.Features.Orders.GetOrderById;
using Payment.Application.Features.Orders.PrepareCheckout;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Xunit;

namespace Payment.Application.Tests;

public class OrderOwnershipTests
{
    [Fact]
    public async Task GetOrderById_WhenCallerIsNotParticipant_ReturnsNotFound()
    {
        var order = CreateOrder();
        var repository = CreateRepository(order);
        var handler = new GetOrderByIdQueryHandler(repository, mapper: null!);

        var result = await handler.Handle(
            new GetOrderByIdQuery(order.Id, Guid.NewGuid(), CanViewAll: false),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Order.NotFound", result.Error?.Code);
    }

    [Fact]
    public async Task PrepareCheckout_WhenCallerIsNotBuyer_ReturnsNotFoundWithoutUpdating()
    {
        var order = CreateOrder();
        var repository = CreateRepository(order);
        var handler = new PrepareCheckoutCommandHandler(
            repository,
            mapper: null!,
            unitOfWork: null!,
            auditPublisher: null!,
            logger: null!);

        var result = await handler.Handle(
            new PrepareCheckoutCommand(
                order.Id,
                Guid.NewGuid(),
                "{\"addressLine1\":\"123 Test Street\"}",
                BuyerNotes: null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Order.NotFound", result.Error?.Code);
        Assert.Equal(0, ((OrderRepositoryProxy)(object)repository).UpdateCalls);
    }

    private static Order CreateOrder() => Order.Create(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "buyer",
        Guid.NewGuid(),
        "seller",
        "Auction item",
        100m);

    private static IOrderRepository CreateRepository(Order order)
    {
        var repository = DispatchProxy.Create<IOrderRepository, OrderRepositoryProxy>();
        ((OrderRepositoryProxy)(object)repository).Order = order;
        return repository;
    }

    public class OrderRepositoryProxy : DispatchProxy
    {
        public Order? Order { get; set; }
        public int UpdateCalls { get; private set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            targetMethod?.Name switch
            {
                nameof(IOrderRepository.GetByIdAsync) => (object)Task.FromResult(Order),
                nameof(IOrderRepository.UpdateAsync) => (object)TrackUpdate((Order)args![0]!),
                _ => throw new NotSupportedException(
                    $"Repository member '{targetMethod?.Name}' is not used by this test")
            };

        private Task<Order> TrackUpdate(Order order)
        {
            UpdateCalls++;
            return Task.FromResult(order);
        }
    }
}
