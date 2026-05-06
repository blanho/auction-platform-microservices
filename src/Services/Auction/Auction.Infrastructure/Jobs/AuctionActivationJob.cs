using Auctions.Domain.Constants;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Auctions.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class AuctionActivationJob : BaseJob
{
    public const string JobId = "auction-activation";
    public const string Description = "Activates scheduled auctions at their start time";

    public AuctionActivationJob(
        ILogger<AuctionActivationJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
        
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var schedulerRepository = scopedProvider.GetRequiredService<IAuctionSchedulerRepository>();
        var writeRepository = scopedProvider.GetRequiredService<IAuctionWriteRepository>();
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();
        var dbContext = scopedProvider.GetRequiredService<AuctionDbContext>();

        var scheduledAuctions = await schedulerRepository.GetScheduledAuctionsToActivateAsync(cancellationToken);

        if (scheduledAuctions.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} scheduled auctions to activate", scheduledAuctions.Count);
        var activatedCount = 0;
        var failedCount = 0;
        var pendingChanges = 0;

        foreach (var auction in scheduledAuctions)
        {
            try
            {
                auction.ChangeStatus(Status.Live);
                await writeRepository.UpdateAsync(auction, cancellationToken);

                activatedCount++;
                pendingChanges++;

                if (pendingChanges >= AuctionDefaults.Batch.SaveBatchSize)
                {
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    dbContext.ChangeTracker.Clear();
                    pendingChanges = 0;
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                Logger.LogError(ex, "Failed to activate auction {AuctionId}", auction.Id);
            }
        }

        if (pendingChanges > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
        }

        Logger.LogInformation("Activated {ActivatedCount} scheduled auctions, {FailedCount} failed out of {TotalCount}",
            activatedCount, failedCount, scheduledAuctions.Count);
    }
}

