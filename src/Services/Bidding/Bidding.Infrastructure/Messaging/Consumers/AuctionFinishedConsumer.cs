
using Bidding.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using AuctionService.Contracts.Events;
using AppUnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

namespace Bidding.Infrastructure.Messaging.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
{
    private readonly IAutoBidRepository _autoBidRepository;
    private readonly AppUnitOfWork _unitOfWork;
    private readonly ILogger<AuctionFinishedConsumer> _logger;

    public AuctionFinishedConsumer(
        IAutoBidRepository autoBidRepository,
        AppUnitOfWork unitOfWork,
        ILogger<AuctionFinishedConsumer> logger)
    {
        _autoBidRepository = autoBidRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Consuming AuctionFinishedEvent for auction {AuctionId}, ItemSold: {ItemSold}",
            message.AuctionId,
            message.ItemSold);

        if (message.ItemSold)
        {
            _logger.LogInformation(
                "Auction {AuctionId} finished - sold to {Winner} for {Amount}",
                message.AuctionId,
                message.WinnerUsername,
                message.SoldAmount);
        }
        else
        {
            _logger.LogInformation(
                "Auction {AuctionId} finished - reserve price not met",
                message.AuctionId);
        }

        await DeactivateAutoBidsForFinishedAuction(message.AuctionId, context.CancellationToken);
    }

    private async Task DeactivateAutoBidsForFinishedAuction(Guid auctionId, CancellationToken cancellationToken)
    {
        try
        {
            var activeAutoBids = await _autoBidRepository.GetActiveAutoBidsForAuctionAsync(
                auctionId, 
                cancellationToken);

            if (activeAutoBids.Count == 0)
            {
                _logger.LogInformation(
                    "No active auto-bids found for finished auction {AuctionId}",
                    auctionId);
                return;
            }

            foreach (var autoBid in activeAutoBids)
            {
                autoBid.Deactivate();
            }

            await _autoBidRepository.UpdateRangeAsync(activeAutoBids, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Deactivated {Count} auto-bid(s) for finished auction {AuctionId}",
                activeAutoBids.Count,
                auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deactivating auto-bids for finished auction {AuctionId}",
                auctionId);
            throw;
        }
    }
}
