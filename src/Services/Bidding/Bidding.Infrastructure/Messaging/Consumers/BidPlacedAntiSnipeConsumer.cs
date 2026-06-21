using Bidding.Application.Interfaces;
using Bidding.Domain.Constants;
using BidService.Contracts.Events;
using BuildingBlocks.Application.Abstractions.Providers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class BidPlacedAntiSnipeConsumer : IConsumer<HighestBidUpdatedEvent>
{
    private readonly IAuctionGrpcClient _auctionGrpcClient;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<BidPlacedAntiSnipeConsumer> _logger;

    public BidPlacedAntiSnipeConsumer(
        IAuctionGrpcClient auctionGrpcClient,
        IDateTimeProvider dateTime,
        ILogger<BidPlacedAntiSnipeConsumer> logger)
    {
        _auctionGrpcClient = auctionGrpcClient;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<HighestBidUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing anti-snipe check for auction {AuctionId} from bid {BidId}", message.AuctionId, message.Id);

        try
        {
            var auctionDetails = await _auctionGrpcClient.GetAuctionDetailsAsync(message.AuctionId, context.CancellationToken);
            if (auctionDetails == null)
            {
                _logger.LogWarning("Auction details not found for anti-snipe check on auction {AuctionId}", message.AuctionId);
                return;
            }

            var timeRemaining = auctionDetails.EndTime - _dateTime.UtcNow;
            if (timeRemaining <= TimeSpan.FromMinutes(BidDefaults.AntiSnipeThresholdMinutes) &&
                timeRemaining > TimeSpan.Zero)
            {
                var newEndTime = auctionDetails.EndTime.AddMinutes(BidDefaults.AntiSnipeExtensionMinutes);
                var result = await _auctionGrpcClient.ExtendAuctionAsync(message.AuctionId, newEndTime, context.CancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Auction {AuctionId} extended to {NewEndTime} due to anti-snipe rule triggered by bid {BidId}",
                        message.AuctionId,
                        result.NewEndTime,
                        message.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to extend auction {AuctionId} for anti-snipe: {Message}", message.AuctionId, result.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing anti-snipe for auction {AuctionId}", message.AuctionId);
            throw;
        }
    }
}
