using Auction = Auctions.Domain.Entities.Auction;
using Auctions.Application.Errors;
using Auctions.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.BulkUpdateAuctions;

public class BulkUpdateAuctionsCommandHandler : ICommandHandler<BulkUpdateAuctionsCommand, int>
{
    private readonly IAuctionReadRepository _readRepository;
    private readonly IAuctionWriteRepository _writeRepository;
    private readonly ILogger<BulkUpdateAuctionsCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public BulkUpdateAuctionsCommandHandler(
        IAuctionReadRepository readRepository,
        IAuctionWriteRepository writeRepository,
        ILogger<BulkUpdateAuctionsCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<Result<int>> Handle(BulkUpdateAuctionsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bulk updating {Count} auctions, Activate: {Activate}", 
            request.AuctionIds.Count, request.Activate);

        try
        {
            var updatedCount = 0;

            foreach (var auctionId in request.AuctionIds)
            {
                var auction = await _readRepository.GetByIdAsync(auctionId, cancellationToken);
                if (auction == null)
                {
                    _logger.LogWarning("Auction {AuctionId} not found, skipping", auctionId);
                    continue;
                }

                if (TryApplyStatusChange(auction, request.Activate))
                {
                    await _writeRepository.UpdateAsync(auction, cancellationToken);
                    updatedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated {UpdatedCount} auctions", updatedCount);
            return Result.Success(updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk update auctions");
            return Result.Failure<int>(AuctionErrors.Auction.BulkUpdateFailed(ex.Message));
        }
    }

    private bool TryApplyStatusChange(Auction auction, bool activate)
    {
        if (activate)
        {
            if ((auction.Status == Status.Inactive || auction.Status == Status.Scheduled) && auction.AuctionEnd > _dateTime.UtcNow)
            {
                auction.ChangeStatus(Status.Live);
                return true;
            }
        }
        else
        {
            if (auction.Status == Status.Live || auction.Status == Status.Scheduled)
            {
                auction.ChangeStatus(Status.Inactive);
                return true;
            }
        }

        return false;
    }
}

