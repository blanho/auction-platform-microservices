using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Infrastructure.Jobs;

public class CheckAuctionFinishedJob : BaseJob
{
    public const string JobId = "check-auction-finished";
    public const string Description = "Checks for finished auctions and marks them as finished";
    
    public CheckAuctionFinishedJob(
        ILogger<CheckAuctionFinishedJob> logger,
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

        var finishedAuctions = await repository.GetFinishedAuctionsAsync(cancellationToken);

        if (finishedAuctions.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} finished auctions to process", finishedAuctions.Count);
        var processedCount = 0;

        foreach (var auction in finishedAuctions)
        {
            try
            {
                if (auction.Status == Status.Finished || auction.Status == Status.ReservedNotMet)
                {
                    Logger.LogDebug("Auction {AuctionId} already finished, skipping", auction.Id);
                    continue;
                }

                var itemSold = auction.CurrentHighBid != null && auction.CurrentHighBid >= auction.ReservePrice;
                auction.Finish(auction.WinnerId, auction.WinnerUsername, auction.CurrentHighBid, itemSold);

                await repository.UpdateAsync(auction, cancellationToken);
                processedCount++;

                Logger.LogInformation(
                    "Marked auction {AuctionId} as finished, Sold: {ItemSold}, Winner: {Winner}",
                    auction.Id,
                    itemSold,
                    auction.WinnerUsername);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to finish auction {AuctionId}", auction.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Finished processing {ProcessedCount}/{TotalCount} auctions", 
            processedCount, finishedAuctions.Count);
    }
}

