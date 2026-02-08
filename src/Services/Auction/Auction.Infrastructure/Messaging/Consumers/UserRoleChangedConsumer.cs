using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Web.Authorization;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Contracts.Events;
using Auction = Auctions.Domain.Entities.Auction;

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

        if (hadSellerRole || hasAdminRole)
        {
            _logger.LogInformation(
                "User {UserId} role changed - Seller/Admin role retained or granted",
                message.UserId);
            return;
        }

        await CancelActiveAuctionsForUser(userId, message, context.CancellationToken);
    }

    private async Task CancelActiveAuctionsForUser(Guid userId, UserRoleChangedEvent message, CancellationToken cancellationToken)
    {
        var activeAuctions = await _readRepository.GetActiveAuctionsBySellerIdAsync(userId, cancellationToken);

        if (!activeAuctions.Any())
        {
            _logger.LogInformation("User {UserId} lost Seller role but has no active auctions", message.UserId);
            return;
        }

        _logger.LogWarning(
            "User {UserId} lost Seller role but has {Count} active auctions. Cancelling them.",
            message.UserId,
            activeAuctions.Count);

        var affectedBidderAuctions = new Dictionary<string, List<string>>();
        var cancelledCount = 0;

        foreach (var auction in activeAuctions)
        {
            CollectAffectedBidder(auction, affectedBidderAuctions);
            auction.Cancel("Seller privileges revoked");
            await _writeRepository.UpdateAsync(auction, cancellationToken);
            cancelledCount++;

            _logger.LogWarning("Cancelled auction {AuctionId} ({Title}) due to role change", auction.Id, auction.Item?.Title ?? "Unknown");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await NotifyAffectedBidders(affectedBidderAuctions, "Seller account privileges changed", cancellationToken);

        _logger.LogWarning(
            "Cancelled {Count} active auctions for user {UserId} due to role change. Affected bidders: {BidderCount}",
            cancelledCount, message.UserId, affectedBidderAuctions.Count);
    }

    private static void CollectAffectedBidder(Auction auction, Dictionary<string, List<string>> affectedBidderAuctions)
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

    private async Task NotifyAffectedBidders(Dictionary<string, List<string>> affectedBidderAuctions, string reason, CancellationToken cancellationToken)
    {
        foreach (var (bidderUsername, auctionTitles) in affectedBidderAuctions)
        {
            foreach (var auctionTitle in auctionTitles)
            {
                await _eventPublisher.PublishAsync(new AuctionCancelledNotificationEvent
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
