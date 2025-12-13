using AuctionService.Application.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
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
        var eventPublisher = scopedProvider.GetRequiredService<IEventPublisher>();

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
                auction.Status = Status.Live;
                await repository.UpdateAsync(auction, cancellationToken);

                var itemTitle = auction.Item != null 
                    ? $"{auction.Item.Year} {auction.Item.Make} {auction.Item.Model}" 
                    : "Auction";

                var auctionStartedEvent = new AuctionStartedEvent
                {
                    AuctionId = auction.Id,
                    Seller = auction.Seller,
                    Title = itemTitle,
                    StartTime = DateTime.UtcNow,
                    EndTime = auction.AuctionEnd.DateTime,
                    ReservePrice = auction.ReversePrice
                };

                await eventPublisher.PublishAsync(auctionStartedEvent, cancellationToken);

                activatedCount++;
                Logger.LogInformation(
                    "Activated scheduled auction {AuctionId}: {Title}",
                    auction.Id, auctionStartedEvent.Title);
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
