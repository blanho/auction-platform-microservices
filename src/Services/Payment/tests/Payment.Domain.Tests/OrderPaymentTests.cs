using BuildingBlocks.Domain.Exceptions;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Events;
using Xunit;

namespace Payment.Domain.Tests;

public class OrderPaymentTests
{
    [Fact]
    public void CompletePayment_WhenDeliveredTwiceForSameTransaction_IsIdempotent()
    {
        var order = CreateOrder();

        var firstDeliveryChangedOrder = order.CompletePayment("pi_123");
        var eventsAfterFirstDelivery = order.DomainEvents.Count;
        var paidAtAfterFirstDelivery = order.PaidAt;
        var secondDeliveryChangedOrder = order.CompletePayment("pi_123");

        Assert.True(firstDeliveryChangedOrder);
        Assert.False(secondDeliveryChangedOrder);
        Assert.Equal(eventsAfterFirstDelivery, order.DomainEvents.Count);
        Assert.Equal(paidAtAfterFirstDelivery, order.PaidAt);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(PaymentStatus.Completed, order.PaymentStatus);
        Assert.Equal("pi_123", order.PaymentTransactionId);
        Assert.Single(order.DomainEvents.OfType<PaymentCompletedDomainEvent>());
    }

    [Fact]
    public void CompletePayment_WhenAlreadyPaidByAnotherTransaction_RejectsConflict()
    {
        var order = CreateOrder();
        order.CompletePayment("pi_original");

        var exception = Assert.Throws<DomainInvariantException>(
            () => order.CompletePayment("pi_conflicting"));

        Assert.Contains("different transaction", exception.Message);
        Assert.Equal("pi_original", order.PaymentTransactionId);
        Assert.Single(order.DomainEvents.OfType<PaymentCompletedDomainEvent>());
    }

    [Fact]
    public void Cancel_WhenPaymentIsPending_CancelsOrder()
    {
        var order = CreateOrder();

        order.Cancel("Buyer changed their mind");

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
    }

    [Fact]
    public void Cancel_WhenPaymentAlreadySucceeded_RequiresRefund()
    {
        var order = CreateOrder();
        order.CompletePayment("pi_123");
        var eventsAfterSuccess = order.DomainEvents.Count;

        var exception = Assert.Throws<InvalidEntityStateException>(() => order.Cancel());

        Assert.Contains("refund", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(eventsAfterSuccess, order.DomainEvents.Count);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(PaymentStatus.Completed, order.PaymentStatus);
        Assert.Equal("pi_123", order.PaymentTransactionId);
    }

    private static Order CreateOrder()
    {
        return Order.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "buyer",
            Guid.NewGuid(),
            "seller",
            "Auction item",
            100m);
    }
}
