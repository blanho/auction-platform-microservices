using BuildingBlocks.Domain.Exceptions;
using Payment.Domain.Entities;
using Xunit;

namespace Payment.Domain.Tests;

public class BuyNowPaymentHoldTests
{
    [Fact]
    public void SagaOrderCannotBePaidUntilItsOwnPurchaseCompletes()
    {
        var correlation = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "buyer", Guid.NewGuid(), "seller", "item", 100,
            buyNowCorrelationId: correlation);
        Assert.Throws<DomainInvariantException>(() => order.CompletePayment("payment"));
        Assert.Throws<DomainInvariantException>(() => order.ConfirmBuyNow(Guid.NewGuid()));
        order.ConfirmBuyNow(correlation);
        Assert.True(order.CompletePayment("payment"));
    }

    [Fact]
    public void CancelledOrderCannotBeActivatedByALateSuccess()
    {
        var correlation = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "buyer", Guid.NewGuid(), "seller", "item", 100,
            buyNowCorrelationId: correlation);
        order.Cancel();
        Assert.Throws<DomainInvariantException>(() => order.ConfirmBuyNow(correlation));
    }
}
