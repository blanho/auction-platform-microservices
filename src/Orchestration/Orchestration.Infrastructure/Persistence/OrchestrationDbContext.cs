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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuyNowSagaState>(s =>
        {
            s.ToTable("BuyNowSagas");
            s.HasKey(x => x.CorrelationId);
            s.Property(x => x.CurrentState).HasMaxLength(80);
            s.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
            s.HasIndex(x => x.TimeoutAt);
        });
        modelBuilder.Entity<AuctionCompletionSagaState>(s =>
        {
            s.ToTable("AuctionCompletionSagas");
            s.HasKey(x => x.CorrelationId);
            s.Property(x => x.CurrentState).HasMaxLength(80);
            s.Property(x => x.RowVersion).HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
            s.HasIndex(x => x.TimeoutAt);
        });
        modelBuilder.Entity<SagaRecoveryCase>().HasKey(x => new { x.Workflow, x.CorrelationId, x.Step });
        modelBuilder.Entity<SagaRecoveryCase>().HasIndex(x => x.ResolvedAt);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
