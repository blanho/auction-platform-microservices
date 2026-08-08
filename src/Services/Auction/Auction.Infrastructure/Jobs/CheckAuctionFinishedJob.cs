using Auctions.Domain.Enums;
using Auctions.Infrastructure.Persistence;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Auctions.Infrastructure.Jobs;

[DisallowConcurrentExecution]
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
        var readRepository = scopedProvider.GetRequiredService<IAuctionReadRepository>();
        var writeRepository = scopedProvider.GetRequiredService<IAuctionWriteRepository>();
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();
        var dbContext = scopedProvider.GetRequiredService<AuctionDbContext>();
        var bidFinalizationClient = scopedProvider.GetRequiredService<IBidFinalizationClient>();

        var finishedAuctions = await readRepository.GetFinishedAuctionsAsync(cancellationToken);

        if (finishedAuctions.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} finished auctions to process", finishedAuctions.Count);
        var processedCount = 0;
        var failedCount = 0;
        var failedAuctionIds = new List<Guid>();

        foreach (var auction in finishedAuctions)
        {
            try
            {
                if (auction.Status == Status.Finished || auction.Status == Status.ReservedNotMet)
                {
                    continue;
                }

                var winningBid = await bidFinalizationClient.FinalizeAuctionAsync(
                    auction.Id,
                    cancellationToken);
                var itemSold = winningBid is not null && winningBid.Amount >= auction.ReservePrice;

                auction.Finish(
                    winningBid?.BidderId,
                    winningBid?.BidderUsername,
                    winningBid?.Amount,
                    itemSold);

                await writeRepository.UpdateAsync(auction, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();
                processedCount++;
            }
            catch (Exception ex)
            {
                failedCount++;
                failedAuctionIds.Add(auction.Id);
                Logger.LogError(ex, "Failed to finish auction {AuctionId}", auction.Id);
            }
        }

        Logger.LogInformation("Finished processing auctions: {ProcessedCount} succeeded, {FailedCount} failed out of {TotalCount}",
            processedCount, failedCount, finishedAuctions.Count);

        if (failedAuctionIds.Count > 0)
        {
            Logger.LogWarning("Failed auction IDs: {FailedIds}", string.Join(", ", failedAuctionIds));
        }
    }
}
