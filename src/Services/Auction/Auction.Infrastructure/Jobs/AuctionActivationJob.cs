using BuildingBlocks.Domain.Enums;
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
        var repository = scopedProvider.GetRequiredService<IAuctionRepository>();
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();

        var scheduledAuctions = await repository.GetScheduledAuctionsToActivateAsync(cancellationToken);

        if (scheduledAuctions.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} scheduled auctions to activate", scheduledAuctions.Count);
        var activatedCount = 0;
        var failedCount = 0;

        foreach (var auction in scheduledAuctions)
        {
            try
            {
                auction.ChangeStatus(Status.Live);
                await repository.UpdateAsync(auction, cancellationToken);

                activatedCount++;
                Logger.LogInformation(
                    "Activated scheduled auction {AuctionId}",
                    auction.Id);
            }
            catch (Exception ex)
            {
                failedCount++;
                Logger.LogError(ex, "Failed to activate auction {AuctionId}", auction.Id);
            }
        }

        if (activatedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        Logger.LogInformation("Activated {ActivatedCount} scheduled auctions, {FailedCount} failed out of {TotalCount}",
            activatedCount, failedCount, scheduledAuctions.Count);
    }
}

