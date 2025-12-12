using AuctionService.Application.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Scheduling.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.Jobs;

/// <summary>
/// Sends notifications when auctions are ending soon
/// Notifies watchers and bidders at configurable intervals (1 hour, 15 min, 5 min before end)
/// Runs every minute to ensure timely notifications
/// </summary>
public class AuctionEndingSoonNotificationJob : BaseJob
{
    public const string JobId = "auction-ending-soon";
    public const string Description = "Sends notifications for auctions ending soon";

    private static readonly TimeSpan[] NotificationThresholds = new[]
    {
        TimeSpan.FromHours(1),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(5)
    };

    public AuctionEndingSoonNotificationJob(
        ILogger<AuctionEndingSoonNotificationJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var repository = scopedProvider.GetRequiredService<IAuctionRepository>();
        var watchlistRepository = scopedProvider.GetRequiredService<IWatchlistRepository>();
        var eventPublisher = scopedProvider.GetRequiredService<IEventPublisher>();

        var utcNow = DateTime.UtcNow;
        var notificationsSent = 0;

        foreach (var threshold in NotificationThresholds)
        {
            var windowStart = utcNow.Add(threshold).AddSeconds(-30);
            var windowEnd = utcNow.Add(threshold).AddSeconds(30);

            var endingSoonAuctions = await repository.GetAuctionsEndingBetweenAsync(
                windowStart, windowEnd, cancellationToken);

            foreach (var auction in endingSoonAuctions)
            {
                if (auction.Status != Status.Live)
                {
                    continue;
                }

                try
                {
                    var watchers = await watchlistRepository.GetWatchersForAuctionAsync(
                        auction.Id, notifyOnEnd: true, cancellationToken);

                    if (watchers.Count == 0)
                    {
                        continue;
                    }

                    var itemTitle = auction.Item != null 
                        ? $"{auction.Item.Year} {auction.Item.Make} {auction.Item.Model}" 
                        : "Auction";

                    var timeRemaining = GetTimeRemainingText(threshold);
                    var endingSoonEvent = new AuctionEndingSoonEvent
                    {
                        AuctionId = auction.Id,
                        Title = itemTitle,
                        CurrentHighBid = auction.CurrentHighBid ?? 0,
                        EndTime = auction.AuctionEnd.DateTime,
                        TimeRemaining = timeRemaining,
                        WatcherUsernames = watchers.Select(w => w.Username).ToList()
                    };

                    await eventPublisher.PublishAsync(endingSoonEvent, cancellationToken);
                    notificationsSent++;

                    Logger.LogDebug(
                        "Sent ending soon notification for auction {AuctionId}, {TimeRemaining} remaining, {WatcherCount} watchers",
                        auction.Id, timeRemaining, watchers.Count);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        "Failed to send ending soon notification for auction {AuctionId}",
                        auction.Id);
                }
            }
        }

        if (notificationsSent > 0)
        {
            Logger.LogInformation("Sent {Count} auction ending soon notifications", notificationsSent);
        }
    }

    private static string GetTimeRemainingText(TimeSpan threshold)
    {
        if (threshold.TotalHours >= 1)
            return $"{(int)threshold.TotalHours} hour";
        return $"{(int)threshold.TotalMinutes} minutes";
    }
}
