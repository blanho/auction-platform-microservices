using BuildingBlocks.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Auctions.Infrastructure.Jobs;

[DisallowConcurrentExecution]
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
        var bookmarkRepository = scopedProvider.GetRequiredService<IBookmarkRepository>();
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
                    var watchers = await bookmarkRepository.GetUsersWatchingAuctionAsync(
                        auction.Id, notifyOnEnd: true, cancellationToken);

                    if (watchers.Count == 0)
                    {
                        continue;
                    }

                    var itemTitle = auction.Item?.Title ?? "Auction";

                    var timeRemaining = GetTimeRemainingText(threshold);
                    var endingSoonEvent = new AuctionEndingSoonEvent
                    {
                        AuctionId = auction.Id,
                        Title = itemTitle,
                        CurrentHighBid = auction.CurrentHighBid ?? 0,
                        EndTime = auction.AuctionEnd.DateTime,
                        TimeRemaining = timeRemaining,
                        WatcherUsernames = watchers
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

