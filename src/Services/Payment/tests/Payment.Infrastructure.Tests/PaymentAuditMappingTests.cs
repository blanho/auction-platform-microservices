using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Infrastructure.Persistence;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class PaymentAuditMappingTests
{
    [Fact]
    public void NewOrdersAndWalletsProvideTheRequiredUpdatedAtValue()
    {
        using var context = new PaymentDbContext(new DbContextOptionsBuilder<PaymentDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=test;Password=test")
            .Options);

        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "buyer",
            Guid.NewGuid(), "seller", "item", 100m);
        var wallet = Wallet.Create(Guid.NewGuid(), "buyer");

        Assert.False(context.Model.FindEntityType(typeof(Order))!
            .FindProperty(nameof(Order.UpdatedAt))!.IsNullable);
        Assert.False(context.Model.FindEntityType(typeof(Wallet))!
            .FindProperty(nameof(Wallet.UpdatedAt))!.IsNullable);
        Assert.Equal(order.CreatedAt, order.UpdatedAt);
        Assert.Equal(wallet.CreatedAt, wallet.UpdatedAt);
    }
}
