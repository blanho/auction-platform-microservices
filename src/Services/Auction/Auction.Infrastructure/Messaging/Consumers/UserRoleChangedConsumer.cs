using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Web.Authorization;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Events;

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

        var hadSellerRole = message.Roles.Contains(AppRoles.Seller);
        var hasAdminRole = message.Roles.Contains(AppRoles.Admin);

        if (!hadSellerRole && !hasAdminRole)
        {
            var activeAuctions = await _readRepository.GetActiveAuctionsBySellerIdAsync(
                userId,
                context.CancellationToken);

            if (activeAuctions.Any())
            {
                _logger.LogWarning(
                    "User {UserId} lost Seller role but has {Count} active auctions. Cancelling them.",
                    message.UserId,
                    activeAuctions.Count);

                var cancelledCount = 0;
                var affectedBidders = new HashSet<string>();

                foreach (var auction in activeAuctions)
                {
                    var hadBids = auction.CurrentHighBid.HasValue;
                    var currentWinner = auction.WinnerUsername;

                    auction.Cancel("Seller privileges revoked");
                    await _writeRepository.UpdateAsync(auction, context.CancellationToken);

                    cancelledCount++;

                    if (hadBids && !string.IsNullOrEmpty(currentWinner))
                    {
                        affectedBidders.Add(currentWinner);
                    }

                    _logger.LogWarning(
                        "Cancelled auction {AuctionId} ({Title}) due to role change",
                        auction.Id,
                        auction.Item?.Title ?? "Unknown");
                }

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);

                foreach (var bidderUsername in affectedBidders)
                {
                    await _eventPublisher.PublishAsync(new AuctionCancelledNotificationEvent
                    {
                        RecipientUsername = bidderUsername,
                        AuctionTitle = activeAuctions.First().Item?.Title ?? "Auction",
                        Reason = "Seller account privileges changed",
                        RefundExpected = true
                    }, context.CancellationToken);
                }

                _logger.LogWarning(
                    "Cancelled {Count} active auctions for user {UserId} due to role change. Affected bidders: {BidderCount}",
                    cancelledCount,
                    message.UserId,
                    affectedBidders.Count);
            }
            else
            {
                _logger.LogInformation(
                    "User {UserId} lost Seller role but has no active auctions",
                    message.UserId);
            }
        }
        else
        {
            _logger.LogInformation(
                "User {UserId} role changed - Seller/Admin role retained or granted",
                message.UserId);
        }
    }
}
