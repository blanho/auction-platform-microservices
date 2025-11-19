using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orchestration.Sagas.BuyNow;

namespace Auctions.Infrastructure.Persistence.Configurations;

public class BuyNowSagaStateConfiguration : SagaClassMap<BuyNowSagaState>
{
    protected override void Configure(EntityTypeBuilder<BuyNowSagaState> entity, ModelBuilder model)
    {
        entity.ToTable("BuyNowSagaStates");

        entity.HasKey(x => x.CorrelationId);

        entity.Property(x => x.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        entity.Property(x => x.BuyerUsername)
            .HasMaxLength(256);

        entity.Property(x => x.SellerUsername)
            .HasMaxLength(256);

        entity.Property(x => x.ItemTitle)
            .HasMaxLength(512);

        entity.Property(x => x.FailureReason)
            .HasMaxLength(2000);

        entity.Property(x => x.BuyNowPrice)
            .HasPrecision(18, 2);

        entity.HasIndex(x => x.AuctionId);
        entity.HasIndex(x => x.BuyerId);
        entity.HasIndex(x => x.CurrentState);
    }
}
