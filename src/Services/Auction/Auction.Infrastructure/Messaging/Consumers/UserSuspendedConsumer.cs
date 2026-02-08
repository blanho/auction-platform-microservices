using BuildingBlocks.Application.Abstractions.Messaging;
using Auctions.Domain.Enums;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class UserSuspendedConsumer : IConsumer<UserSuspendedEvent>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserSuspendedConsumer> _logger;

    public UserSuspendedConsumer(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<UserSuspendedConsumer> logger)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserSuspendedEvent> context)
    {
        var message = context.Message;
        _logger.LogWarning(
            "Processing UserSuspendedEvent for user {UserId} ({Username}). Reason: {Reason}",
            message.UserId,
            message.Username,
            message.Reason);

        var activeAuctions = await _readRepository.GetActiveAuctionsBySellerIdAsync(
            Guid.Parse(message.UserId),
            context.CancellationToken);

        if (!activeAuctions.Any())
        {
            _logger.LogInformation("No active auctions found for suspended user {UserId}", message.UserId);
            return;
        }

        var cancelledCount = 0;
        var affectedBidderAuctions = new Dictionary<string, List<string>>();

        foreach (var auction in activeAuctions)
        {
            var hadBids = auction.CurrentHighBid.HasValue;
            var currentWinner = auction.WinnerUsername;
            var auctionTitle = auction.Item?.Title ?? "Auction";

            auction.Cancel($"Seller account suspended: {message.Reason}");
            await _writeRepository.UpdateAsync(auction, context.CancellationToken);
            
            cancelledCount++;

            if (hadBids && !string.IsNullOrEmpty(currentWinner))
            {
                if (!affectedBidderAuctions.TryGetValue(currentWinner, out var titles))
                {
                    titles = new List<string>();
                    affectedBidderAuctions[currentWinner] = titles;
                }
                titles.Add(auctionTitle);
            }

            _logger.LogWarning(
                "Cancelled auction {AuctionId} ({Title}) - Had bids: {HadBids}",
                auction.Id,
                auctionTitle,
                hadBids);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        foreach (var (bidderUsername, auctionTitles) in affectedBidderAuctions)
        {
            foreach (var auctionTitle in auctionTitles)
            {
                await _eventPublisher.PublishAsync(new AuctionCancelledNotificationEvent
                {
                    RecipientUsername = bidderUsername,
                    AuctionTitle = auctionTitle,
                    Reason = "Seller account suspended",
                    RefundExpected = true
                }, context.CancellationToken);
            }
        }

        _logger.LogWarning(
            "Cancelled {Count} active auctions for suspended user {UserId}. Affected bidders: {BidderCount}",
            cancelledCount,
            message.UserId,
            affectedBidderAuctions.Count);
    }
}
