using AuctionService.Application.Interfaces;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Domain.Enums;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.BulkUpdateAuctions;

public class BulkUpdateAuctionsCommandHandler : ICommandHandler<BulkUpdateAuctionsCommand, int>
{
    private readonly IAuctionRepository _repository;
    private readonly IAppLogger<BulkUpdateAuctionsCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public BulkUpdateAuctionsCommandHandler(
        IAuctionRepository repository,
        IAppLogger<BulkUpdateAuctionsCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime)
    {
        _repository = repository;
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
                var auction = await _repository.GetByIdAsync(auctionId, cancellationToken);

                if (request.Activate)
                {
                    if (auction.Status == Status.Inactive || auction.Status == Status.Scheduled)
                    {
                        if (auction.AuctionEnd > _dateTime.UtcNow)
                        {
                            auction.Status = Status.Live;
                            await _repository.UpdateAsync(auction, cancellationToken);
                            updatedCount++;
                        }
                    }
                }
                else
                {
                    if (auction.Status == Status.Live || auction.Status == Status.Scheduled)
                    {
                        auction.Status = Status.Inactive;
                        await _repository.UpdateAsync(auction, cancellationToken);
                        updatedCount++;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated {UpdatedCount} auctions", updatedCount);
            return Result.Success(updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk update auctions");
            return Result.Failure<int>(Error.Create("Auction.BulkUpdateFailed", $"Failed to bulk update auctions: {ex.Message}"));
        }
    }
}
