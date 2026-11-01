using BuildingBlocks.Application.Abstractions.Messaging;
using Auctions.Domain.Enums;
using IdentityService.Contracts.Events;
using Auctions.Infrastructure.Messaging;

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
        await CancelActiveAuctionsAsync(userId, message, context.CancellationToken);
        await AnonymizeFinishedAuctionsAsync(userId, message, context.CancellationToken);

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        var allAuctions = await _readRepository.GetAllBySellerIdAsync(userId, context.CancellationToken);
        _logger.LogWarning(
            "Completed account deletion processing for user {UserId}. Total auctions affected: {TotalCount}",
            message.UserId,
            allAuctions.Count);
    }

    private async Task CancelActiveAuctionsAsync(Guid userId, UserDeletedEvent message, CancellationToken cancellationToken)
    {
        var activeAuctions = await _readRepository.GetActiveAuctionsBySellerIdAsync(userId, cancellationToken);

        if (activeAuctions.Count == 0)
        {
            return;
        }

        var notifications = new AuctionCancellationNotifications();

        foreach (var auction in activeAuctions)
        {
            notifications.CollectAffectedBidder(auction);
            auction.Cancel("Seller account deleted");
            await _writeRepository.UpdateAsync(auction, cancellationToken);

            _logger.LogWarning(
                "Cancelled auction {AuctionId} ({Title}) - Status: {Status}, Had bids: {HadBids}",
                auction.Id,
                auction.Item?.Title ?? "Unknown",
                auction.Status,
                auction.CurrentHighBid.HasValue);
        }

        await notifications.PublishAsync(_eventPublisher, "Seller account no longer active", cancellationToken);

        _logger.LogWarning(
            "Cancelled {Count} active auctions for deleted user {UserId}. Affected bidders: {BidderCount}",
            activeAuctions.Count,
            message.UserId,
            notifications.BidderCount);
    }

    private async Task AnonymizeFinishedAuctionsAsync(Guid userId, UserDeletedEvent message, CancellationToken cancellationToken)
    {
        var allAuctions = await _readRepository.GetAllBySellerIdAsync(userId, cancellationToken);
        var finishedAuctions = allAuctions
            .Where(a => a.Status == Status.Finished)
            .ToList();

        if (finishedAuctions.Count == 0)
        {
            return;
        }

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
