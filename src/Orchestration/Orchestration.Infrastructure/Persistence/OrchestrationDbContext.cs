using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Sagas.AuctionCompletion;
using Orchestration.Sagas.BuyNow;

namespace Orchestration.Infrastructure.Persistence;

public class OrchestrationDbContext(DbContextOptions<OrchestrationDbContext> options) : DbContext(options)
{
    public DbSet<BuyNowSagaState> BuyNowSagas => Set<BuyNowSagaState>();
    public DbSet<AuctionCompletionSagaState> AuctionCompletionSagas => Set<AuctionCompletionSagaState>();
    public DbSet<SagaRecoveryCase> RecoveryCases => Set<SagaRecoveryCase>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<BuyNowSagaState>(s =>
        {
            s.ToTable("BuyNowSagas");
            s.HasKey(x => x.CorrelationId);
            s.Property(x => x.CurrentState).HasMaxLength(80);
            s.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
            s.HasIndex(x => x.TimeoutAt);
        });
        model.Entity<AuctionCompletionSagaState>(s =>
        {
            s.ToTable("AuctionCompletionSagas");
            s.HasKey(x => x.CorrelationId);
            s.Property(x => x.CurrentState).HasMaxLength(80);
            s.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
            s.HasIndex(x => x.TimeoutAt);
        });
        model.Entity<SagaRecoveryCase>().HasKey(x => new { x.Workflow, x.CorrelationId, x.Step });
        model.Entity<SagaRecoveryCase>().HasIndex(x => x.ResolvedAt);
        model.AddInboxStateEntity();
        model.AddOutboxStateEntity();
        model.AddOutboxMessageEntity();
    }
}
