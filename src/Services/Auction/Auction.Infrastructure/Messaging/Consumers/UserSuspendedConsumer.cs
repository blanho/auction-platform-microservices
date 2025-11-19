using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Domain.Enums;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class UserSuspendedConsumer : IConsumer<UserSuspendedEvent>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserSuspendedConsumer> _logger;

    public UserSuspendedConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<UserSuspendedConsumer> logger)
    {
        _repository = repository;
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

        var activeAuctions = await _repository.GetActiveAuctionsBySellerIdAsync(
            Guid.Parse(message.UserId),
            context.CancellationToken);

        if (!activeAuctions.Any())
        {
            _logger.LogInformation("No active auctions found for suspended user {UserId}", message.UserId);
            return;
        }

        var cancelledCount = 0;
        var affectedBidders = new HashSet<string>();

        foreach (var auction in activeAuctions)
        {
            var hadBids = auction.CurrentHighBid.HasValue;
            var currentWinner = auction.WinnerUsername;

            auction.Cancel($"Seller account suspended: {message.Reason}");
            await _repository.UpdateAsync(auction, context.CancellationToken);
            
            cancelledCount++;

            if (hadBids && !string.IsNullOrEmpty(currentWinner))
            {
                affectedBidders.Add(currentWinner);
            }

            _logger.LogWarning(
                "Cancelled auction {AuctionId} ({Title}) - Had bids: {HadBids}",
                auction.Id,
                auction.Item?.Title ?? "Unknown",
                hadBids);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        foreach (var bidderUsername in affectedBidders)
        {
            await _eventPublisher.PublishAsync(new AuctionCancelledNotificationEvent
            {
                RecipientUsername = bidderUsername,
                AuctionTitle = activeAuctions.First().Item?.Title ?? "Auction",
                Reason = "Seller account suspended",
                RefundExpected = true
            }, context.CancellationToken);
        }

        _logger.LogWarning(
            "Cancelled {Count} active auctions for suspended user {UserId}. Affected bidders: {BidderCount}",
            cancelledCount,
            message.UserId,
            affectedBidders.Count);
    }
}
