using BuildingBlocks.Application.Abstractions.Messaging;
using NotificationService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging;

internal sealed class AuctionCancellationNotifications
{
    private readonly Dictionary<string, List<string>> _auctionTitlesByBidder = new();

    public int BidderCount => _auctionTitlesByBidder.Count;

    public void CollectAffectedBidder(Auction auction, string fallbackTitle = "Unknown")
    {
        if (!auction.CurrentHighBid.HasValue || string.IsNullOrEmpty(auction.WinnerUsername))
        {
            return;
        }

        if (!_auctionTitlesByBidder.TryGetValue(auction.WinnerUsername, out var titles))
        {
            titles = new List<string>();
            _auctionTitlesByBidder[auction.WinnerUsername] = titles;
        }

        titles.Add(auction.Item?.Title ?? fallbackTitle);
    }

    public async Task PublishAsync(IEventPublisher publisher, string reason, CancellationToken cancellationToken)
    {
        foreach (var (bidderUsername, auctionTitles) in _auctionTitlesByBidder)
        {
            foreach (var auctionTitle in auctionTitles)
            {
                await publisher.PublishAsync(new AuctionCancelledNotificationEvent
                {
                    RecipientUsername = bidderUsername,
                    AuctionTitle = auctionTitle,
                    Reason = reason,
                    RefundExpected = true
                }, cancellationToken);
            }
        }
    }
}
