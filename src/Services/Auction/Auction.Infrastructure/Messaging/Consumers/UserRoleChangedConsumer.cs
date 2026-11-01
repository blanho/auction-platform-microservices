using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Authorization;
using IdentityService.Contracts.Events;
using Auctions.Infrastructure.Messaging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class UserRoleChangedConsumer : IConsumer<UserRoleChangedEvent>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UserRoleChangedConsumer> _logger;

    public UserRoleChangedConsumer(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ILogger<UserRoleChangedConsumer> logger)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRoleChangedEvent> context)
    {
        var message = context.Message;
        var userId = Guid.Parse(message.UserId);

        _logger.LogInformation(
            "Processing UserRoleChangedEvent for user {UserId} ({Username}). New roles: {Roles}",
            message.UserId,
            message.Username,
            string.Join(", ", message.Roles));

        var canSell = message.Roles.Contains(Roles.Seller) || message.Roles.Contains(Roles.Admin);

        if (canSell)
        {
            _logger.LogInformation(
                "User {UserId} role changed - Seller/Admin role retained or granted",
                message.UserId);
            return;
        }

        await CancelActiveAuctionsForUserAsync(userId, message, context.CancellationToken);
    }

    private async Task CancelActiveAuctionsForUserAsync(Guid userId, UserRoleChangedEvent message, CancellationToken cancellationToken)
    {
        var activeAuctions = await _readRepository.GetActiveAuctionsBySellerIdAsync(userId, cancellationToken);

        if (activeAuctions.Count == 0)
        {
            _logger.LogInformation("User {UserId} lost Seller role but has no active auctions", message.UserId);
            return;
        }

        _logger.LogWarning(
            "User {UserId} lost Seller role but has {Count} active auctions. Cancelling them.",
            message.UserId,
            activeAuctions.Count);

        var notifications = new AuctionCancellationNotifications();
        var cancelledCount = 0;

        foreach (var auction in activeAuctions)
        {
            notifications.CollectAffectedBidder(auction);
            auction.Cancel("Seller privileges revoked");
            await _writeRepository.UpdateAsync(auction, cancellationToken);
            cancelledCount++;

            _logger.LogWarning("Cancelled auction {AuctionId} ({Title}) due to role change", auction.Id, auction.Item?.Title ?? "Unknown");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await notifications.PublishAsync(_eventPublisher, "Seller account privileges changed", cancellationToken);

        _logger.LogWarning(
            "Cancelled {Count} active auctions for user {UserId} due to role change. Affected bidders: {BidderCount}",
            cancelledCount, message.UserId, notifications.BidderCount);
    }
}
