using BuildingBlocks.Application.Abstractions.Messaging;
using Auctions.Domain.Enums;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Events;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class UserDeletedConsumer : IConsumer<UserDeletedEvent>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserDeletedConsumer> _logger;

    public UserDeletedConsumer(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<UserDeletedConsumer> logger)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
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

        var userId = Guid.Parse(message.UserId);
        await CancelActiveAuctions(userId, message, context.CancellationToken);
        await AnonymizeFinishedAuctions(userId, message, context.CancellationToken);

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        var allAuctions = await _readRepository.GetAllBySellerIdAsync(userId, context.CancellationToken);
        _logger.LogWarning(
            "Completed account deletion processing for user {UserId}. Total auctions affected: {TotalCount}",
            message.UserId,
            allAuctions.Count);
    }

    private async Task CancelActiveAuctions(Guid userId, UserDeletedEvent message, CancellationToken cancellationToken)
    {
        var activeAuctions = await _readRepository.GetActiveAuctionsBySellerIdAsync(userId, cancellationToken);

        if (!activeAuctions.Any())
            return;

        var affectedBidderAuctions = new Dictionary<string, List<string>>();

        foreach (var auction in activeAuctions)
        {
            CollectAffectedBidder(auction, affectedBidderAuctions);
            auction.Cancel("Seller account deleted");
            await _writeRepository.UpdateAsync(auction, cancellationToken);
            
            _logger.LogWarning(
                "Cancelled auction {AuctionId} ({Title}) - Status: {Status}, Had bids: {HadBids}",
                auction.Id,
                auction.Item?.Title ?? "Unknown",
                auction.Status,
                auction.CurrentHighBid.HasValue);
        }

        await NotifyAffectedBidders(affectedBidderAuctions, cancellationToken);

        _logger.LogWarning(
            "Cancelled {Count} active auctions for deleted user {UserId}. Affected bidders: {BidderCount}",
            activeAuctions.Count,
            message.UserId,
            affectedBidderAuctions.Count);
    }

    private static void CollectAffectedBidder(Auctions.Domain.Entities.Auction auction, Dictionary<string, List<string>> affectedBidderAuctions)
    {
        if (!auction.CurrentHighBid.HasValue || string.IsNullOrEmpty(auction.WinnerUsername))
            return;

        var auctionTitle = auction.Item?.Title ?? "Unknown";
        if (!affectedBidderAuctions.TryGetValue(auction.WinnerUsername, out var titles))
        {
            titles = new List<string>();
            affectedBidderAuctions[auction.WinnerUsername] = titles;
        }
        titles.Add(auctionTitle);
    }

    private async Task NotifyAffectedBidders(Dictionary<string, List<string>> affectedBidderAuctions, CancellationToken cancellationToken)
    {
        foreach (var (bidderUsername, auctionTitles) in affectedBidderAuctions)
        {
            foreach (var auctionTitle in auctionTitles)
            {
                await _eventPublisher.PublishAsync(new AuctionCancelledNotificationEvent
                {
                    RecipientUsername = bidderUsername,
                    AuctionTitle = auctionTitle,
                    Reason = "Seller account no longer active",
                    RefundExpected = true
                }, cancellationToken);
            }
        }
    }

    private async Task AnonymizeFinishedAuctions(Guid userId, UserDeletedEvent message, CancellationToken cancellationToken)
    {
        var allAuctions = await _readRepository.GetAllBySellerIdAsync(userId, cancellationToken);
        var finishedAuctions = allAuctions
            .Where(a => a.Status == Status.Finished)
            .ToList();

        if (!finishedAuctions.Any())
            return;

        foreach (var auction in finishedAuctions)
        {
            auction.UpdateSellerUsername($"[Deleted User - {message.Username}]");
            await _writeRepository.UpdateAsync(auction, cancellationToken);
        }

        _logger.LogInformation(
            "Anonymized {Count} finished auctions for deleted user {UserId}",
            finishedAuctions.Count,
            message.UserId);
    }
}
