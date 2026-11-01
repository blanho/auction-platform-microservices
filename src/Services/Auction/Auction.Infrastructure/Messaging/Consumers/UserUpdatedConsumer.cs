using IdentityService.Contracts.Events;

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

        var userId = Guid.Parse(message.UserId);

        var sellerAuctions = await _readRepository.GetAllBySellerIdAsync(
            userId,
            context.CancellationToken);

        var updatedSellerCount = await UpdateSellerUsernamesAsync(
            sellerAuctions, message.Username, context.CancellationToken);

        if (sellerAuctions.Count > 0)
        {
            _logger.LogInformation(
                "Updated seller username in {Count} auctions for user {UserId}",
                sellerAuctions.Count(a => a.SellerUsername == message.Username),
                message.UserId);
        }

        var wonAuctions = await _readRepository.GetAuctionsWithWinnerIdAsync(
            userId,
            context.CancellationToken);

        var updatedWinnerCount = await UpdateWinnerUsernamesAsync(
            wonAuctions, message.Username, context.CancellationToken);

        if (wonAuctions.Count > 0)
        {
            _logger.LogInformation(
                "Updated winner username in {Count} auctions for user {UserId}",
                wonAuctions.Count(a => a.WinnerUsername == message.Username),
                message.UserId);
        }

        var updatedCount = updatedSellerCount + updatedWinnerCount;
        if (updatedCount == 0)
        {
            _logger.LogDebug("No username changes needed for user {UserId}", message.UserId);
            return;
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation(
            "Successfully synced username for user {UserId}. Total records updated: {Count} (Seller: {SellerCount}, Winner: {WinnerCount})",
            message.UserId, updatedCount, sellerAuctions.Count, wonAuctions.Count);
    }

    private async Task<int> UpdateSellerUsernamesAsync(
        IReadOnlyList<Auction> auctions,
        string username,
        CancellationToken cancellationToken)
    {
        var updatedCount = 0;
        foreach (var auction in auctions)
        {
            if (auction.SellerUsername == username)
            {
                continue;
            }

            auction.UpdateSellerUsername(username);
            await _writeRepository.UpdateAsync(auction, cancellationToken);
            updatedCount++;
        }

        return updatedCount;
    }

    private async Task<int> UpdateWinnerUsernamesAsync(
        IReadOnlyList<Auction> auctions,
        string username,
        CancellationToken cancellationToken)
    {
        var updatedCount = 0;
        foreach (var auction in auctions)
        {
            if (auction.WinnerUsername == username)
            {
                continue;
            }

            auction.UpdateWinnerUsername(username);
            await _writeRepository.UpdateAsync(auction, cancellationToken);
            updatedCount++;
        }

        return updatedCount;
    }
}
