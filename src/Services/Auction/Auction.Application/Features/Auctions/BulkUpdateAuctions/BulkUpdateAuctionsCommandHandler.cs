using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace Auctions.Application.Commands.BulkUpdateAuctions;

public class BulkUpdateAuctionsCommandHandler : ICommandHandler<BulkUpdateAuctionsCommand, int>
{
    private readonly IAuctionRepository _repository;
    private readonly ILogger<BulkUpdateAuctionsCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public BulkUpdateAuctionsCommandHandler(
        IAuctionRepository repository,
        ILogger<BulkUpdateAuctionsCommandHandler> logger,
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
                            auction.ChangeStatus(Status.Live);
                            await _repository.UpdateAsync(auction, cancellationToken);
                            updatedCount++;
                        }
                    }
                }
                else
                {
                    if (auction.Status == Status.Live || auction.Status == Status.Scheduled)
                    {
                        auction.ChangeStatus(Status.Inactive);
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

