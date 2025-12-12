using AuctionService.Application.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Scheduling.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.Jobs;

/// <summary>
/// Checks for finished auctions and publishes AuctionFinishedEvent
/// Runs every 5 seconds for time-critical auction endings
/// </summary>
public class CheckAuctionFinishedJob : BaseJob
{
    public const string JobId = "check-auction-finished";
    public const string Description = "Checks for finished auctions and publishes events";
    
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
        var eventPublisher = scopedProvider.GetRequiredService<IEventPublisher>();

        var finishedAuctions = await repository.GetFinishedAuctionsAsync(cancellationToken);

        if (finishedAuctions.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} finished auctions to process", finishedAuctions.Count);

        foreach (var auction in finishedAuctions)
        {
            try
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

                Logger.LogInformation(
                    "Published AuctionFinishedEvent for auction {AuctionId}, Sold: {ItemSold}",
                    auction.Id,
                    auctionFinishedEvent.ItemSold);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to publish event for auction {AuctionId}", auction.Id);
            }
        }
    }
}
