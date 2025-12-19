using AuctionService.Application.Interfaces;
using Common.Domain.Enums;
using Common.Repository.Interfaces;
using Common.Scheduling.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.Jobs;

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

        foreach (var auction in scheduledAuctions)
        {
            try
            {
                auction.ChangeStatus(Status.Live);
                await repository.UpdateAsync(auction, cancellationToken);

                activatedCount++;
                Logger.LogInformation(
                    "Activated scheduled auction {AuctionId}: {Title}",
                    auction.Id, auction.Item?.Title ?? "Unknown");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to activate auction {AuctionId}", auction.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Activated {ActivatedCount}/{TotalCount} scheduled auctions",
            activatedCount, scheduledAuctions.Count);
    }
}
