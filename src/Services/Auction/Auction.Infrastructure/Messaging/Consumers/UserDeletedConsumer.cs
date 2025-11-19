using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Domain.Enums;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserDeletedConsumer> _logger;

    public UserDeletedConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<UserDeletedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogWarning(
            "Processing UserDeletedEvent for user {UserId} ({Username})",
            message.UserId,
            message.Username);

        var activeAuctions = await _repository.GetActiveAuctionsBySellerIdAsync(
            Guid.Parse(message.UserId),
            context.CancellationToken);

        if (activeAuctions.Any())
        {
            var affectedBidders = new HashSet<string>();

            foreach (var auction in activeAuctions)
            {
                var hadBids = auction.CurrentHighBid.HasValue;
                var auctionTitle = auction.Item?.Title ?? "Unknown";

                if (hadBids && !string.IsNullOrEmpty(auction.WinnerUsername))
                {
                    affectedBidders.Add(auction.WinnerUsername);
                }

                auction.Cancel("Seller account deleted");
                await _repository.UpdateAsync(auction, context.CancellationToken);
                
                _logger.LogWarning(
                    "Cancelled auction {AuctionId} ({Title}) - Status: {Status}, Had bids: {HadBids}",
                    auction.Id,
                    auctionTitle,
                    auction.Status,
                    hadBids);
            }

            foreach (var bidderUsername in affectedBidders)
            {
                await _eventPublisher.PublishAsync(new AuctionCancelledNotificationEvent
                {
                    RecipientUsername = bidderUsername,
                    AuctionTitle = activeAuctions.First().Item?.Title ?? "Auction",
                    Reason = "Seller account no longer active",
                    RefundExpected = true
                }, context.CancellationToken);
            }

            _logger.LogWarning(
                "Cancelled {Count} active auctions for deleted user {UserId}. Affected bidders: {BidderCount}",
                activeAuctions.Count,
                message.UserId,
                affectedBidders.Count);
        }

        var allAuctions = await _repository.GetAllBySellerIdAsync(
            Guid.Parse(message.UserId),
            context.CancellationToken);

        var finishedAuctions = allAuctions
            .Where(a => a.Status == Status.Finished)
            .ToList();

        if (finishedAuctions.Any())
        {
            foreach (var auction in finishedAuctions)
            {
                auction.UpdateSellerUsername($"[Deleted User - {message.Username}]");
                await _repository.UpdateAsync(auction, context.CancellationToken);
            }

            _logger.LogInformation(
                "Anonymized {Count} finished auctions for deleted user {UserId}",
                finishedAuctions.Count,
                message.UserId);
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogWarning(
            "Completed account deletion processing for user {UserId}. Total auctions affected: {TotalCount}",
            message.UserId,
            allAuctions.Count);
    }
}
