using AuctionService.Application.Interfaces;
using Common.Core.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuctionService.Infrastructure.BackgroundServices;

public class ScheduledAuctionDeactivationService : BackgroundService
{
    private readonly ILogger<ScheduledAuctionDeactivationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public ScheduledAuctionDeactivationService(
        ILogger<ScheduledAuctionDeactivationService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ScheduledAuctionDeactivationService is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired auctions");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessExpiredAuctionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuctionRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        try
        {
            var expiredAuctions = await repository.GetAuctionsToAutoDeactivateAsync(cancellationToken);
            if (expiredAuctions.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Found {Count} auctions to auto-deactivate", expiredAuctions.Count);

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

                    _logger.LogInformation(
                        "Auto-deactivated auction {AuctionId} from {PreviousStatus} to {FinalStatus}",
                        auction.Id, previousStatus, finalStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to auto-deactivate auction {AuctionId}", auction.Id);
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Completed auto-deactivation of {Count} auctions", expiredAuctions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expired auctions");
        }
    }

    private static Status DetermineFinalStatus(Domain.Entities.Auction auction)
    {
        if (auction.CurrentHighBid.HasValue && auction.CurrentHighBid >= auction.ReversePrice)
        {
            return Status.Finished;
        }
        if (auction.CurrentHighBid.HasValue && auction.CurrentHighBid > 0)
        {
            return Status.ReservedNotMet;
        }
        return Status.ReservedNotMet;
    }
}
