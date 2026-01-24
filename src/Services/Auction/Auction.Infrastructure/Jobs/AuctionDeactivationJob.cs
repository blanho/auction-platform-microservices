using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Infrastructure.Jobs;

public class AuctionDeactivationJob : BaseJob
{
    public const string JobId = "auction-deactivation";
    public const string Description = "Auto-deactivates expired auctions";

    public AuctionDeactivationJob(
        ILogger<AuctionDeactivationJob> logger,
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

        var expiredAuctions = await repository.GetAuctionsToAutoDeactivateAsync(cancellationToken);

        if (expiredAuctions.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} auctions to auto-deactivate", expiredAuctions.Count);
        var processedCount = 0;

        foreach (var auction in expiredAuctions)
        {
            try
            {
                var itemSold = auction.CurrentHighBid != null && auction.CurrentHighBid >= auction.ReservePrice;
                auction.Finish(auction.WinnerId, auction.WinnerUsername, auction.CurrentHighBid, itemSold);

                await repository.UpdateAsync(auction, cancellationToken);

                processedCount++;
                Logger.LogDebug(
                    "Deactivated auction {AuctionId} to {FinalStatus}",
                    auction.Id, auction.Status);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to deactivate auction {AuctionId}", auction.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Completed deactivation of {ProcessedCount}/{TotalCount} auctions",
            processedCount, expiredAuctions.Count);
    }
}

