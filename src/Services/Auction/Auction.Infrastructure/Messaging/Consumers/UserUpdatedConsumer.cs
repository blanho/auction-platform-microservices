using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auctions.Infrastructure.Messaging.Consumers;

public class UserUpdatedConsumer : IConsumer<UserUpdatedEvent>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserUpdatedConsumer> _logger;

    public UserUpdatedConsumer(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserUpdatedConsumer> logger)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
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

        var sellerAuctions = await _readRepository.GetAllBySellerIdAsync(
            userId,
            context.CancellationToken);

        foreach (var auction in sellerAuctions)
        {
            if (auction.SellerUsername != message.Username)
            {
                auction.UpdateSellerUsername(message.Username);
                await _writeRepository.UpdateAsync(auction, context.CancellationToken);
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

        var wonAuctions = await _readRepository.GetAuctionsWithWinnerIdAsync(
            userId,
            context.CancellationToken);

        foreach (var auction in wonAuctions)
        {
            if (auction.WinnerUsername != message.Username)
            {
                auction.UpdateWinnerUsername(message.Username);
                await _writeRepository.UpdateAsync(auction, context.CancellationToken);
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
