using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class UserUpdatedConsumer : IConsumer<UserUpdatedEvent>
{
    private readonly IAuctionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserUpdatedConsumer> _logger;

    public UserUpdatedConsumer(
        IAuctionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UserUpdatedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Processing UserUpdatedEvent for user {UserId}. New username: {Username}",
            message.UserId,
            message.Username);

        var updatedCount = 0;
        var userId = Guid.Parse(message.UserId);

        var sellerAuctions = await _repository.GetAllBySellerIdAsync(
            userId,
            context.CancellationToken);

        foreach (var auction in sellerAuctions)
        {
            if (auction.SellerUsername != message.Username)
            {
                auction.UpdateSellerUsername(message.Username);
                await _repository.UpdateAsync(auction, context.CancellationToken);
                updatedCount++;
            }
        }

        if (sellerAuctions.Any())
        {
            _logger.LogInformation(
                "Updated seller username in {Count} auctions for user {UserId}",
                sellerAuctions.Count(a => a.SellerUsername == message.Username),
                message.UserId);
        }

        var wonAuctions = await _repository.GetAuctionsWithWinnerIdAsync(
            userId,
            context.CancellationToken);

        foreach (var auction in wonAuctions)
        {
            if (auction.WinnerUsername != message.Username)
            {
                auction.UpdateWinnerUsername(message.Username);
                await _repository.UpdateAsync(auction, context.CancellationToken);
                updatedCount++;
            }
        }

        if (wonAuctions.Any())
        {
            _logger.LogInformation(
                "Updated winner username in {Count} auctions for user {UserId}",
                wonAuctions.Count(a => a.WinnerUsername == message.Username),
                message.UserId);
        }

        if (updatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation(
                "Successfully synced username for user {UserId}. Total records updated: {Count} (Seller: {SellerCount}, Winner: {WinnerCount})",
                message.UserId,
                updatedCount,
                sellerAuctions.Count,
                wonAuctions.Count);
        }
        else
        {
            _logger.LogDebug(
                "No username changes needed for user {UserId}",
                message.UserId);
        }
    }
}
