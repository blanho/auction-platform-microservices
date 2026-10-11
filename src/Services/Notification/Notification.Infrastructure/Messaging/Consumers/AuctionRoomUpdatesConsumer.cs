using AuctionService.Contracts.Events;

namespace Notification.Infrastructure.Messaging.Consumers;

public class AuctionRoomUpdatesConsumer(INotificationHubService hub) :
    IConsumer<AuctionHighBidUpdatedEvent>,
    IConsumer<AuctionFinishedEvent>,
    IConsumer<AuctionExtendedEvent>
{
    public Task Consume(ConsumeContext<AuctionHighBidUpdatedEvent> context)
    {
        var message = context.Message;
        return hub.SendBidPlacedToAuctionRoomAsync(message.AuctionId.ToString(), new
        {
            auctionId = message.AuctionId,
            bidId = string.Empty,
            amount = message.BidAmount,
            bidderId = message.BidderId,
            bidderName = message.BidderUsername,
            timestamp = message.UpdatedAt
        });
    }

    public Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;
        return hub.SendAuctionEndedToRoomAsync(message.AuctionId.ToString(), new
        {
            auctionId = message.AuctionId,
            status = "Finished",
            winnerId = message.WinnerId,
            winningBid = message.SoldAmount
        });
    }

    public Task Consume(ConsumeContext<AuctionExtendedEvent> context)
    {
        var message = context.Message;
        return hub.SendAuctionExtendedToRoomAsync(message.AuctionId.ToString(), new
        {
            auctionId = message.AuctionId,
            status = "Live",
            auctionEnd = message.NewEndTime
        });
    }
}
