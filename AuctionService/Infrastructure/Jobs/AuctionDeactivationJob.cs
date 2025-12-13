using AuctionService.Application.Interfaces;
using Common.Core.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;
using Common.Scheduling.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.Jobs;

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
        var eventPublisher = scopedProvider.GetRequiredService<IEventPublisher>();

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
                var finalStatus = DetermineFinalStatus(auction);
                var previousStatus = auction.Status;
                auction.Status = finalStatus;

                await repository.UpdateAsync(auction, cancellationToken);

                if (finalStatus == Status.Finished || finalStatus == Status.ReservedNotMet)
                {
                    var auctionFinishedEvent = new AuctionFinishedEvent
                    {
                        AuctionId = auction.Id,
                        ItemSold = auction.CurrentHighBid != null && auction.CurrentHighBid >= auction.ReversePrice,
                        Winner = auction.Winner,
                        Seller = auction.Seller,
                        SoldAmount = auction.CurrentHighBid
                    };
                    await eventPublisher.PublishAsync(auctionFinishedEvent, cancellationToken);
                }

                processedCount++;
                Logger.LogDebug(
                    "Deactivated auction {AuctionId} from {PreviousStatus} to {FinalStatus}",
                    auction.Id, previousStatus, finalStatus);
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

    private static Status DetermineFinalStatus(Domain.Entities.Auction auction)
    {
        if (auction.CurrentHighBid == null)
        {
            return Status.Inactive;
        }

        return auction.CurrentHighBid >= auction.ReversePrice
            ? Status.Finished
            : Status.ReservedNotMet;
    }
}
