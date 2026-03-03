using Auctions.Application.Errors;
using Auctions.Domain.Enums;
using BuildingBlocks.Domain.Exceptions;

namespace Auctions.Application.Features.Auctions.ExtendAuction;

public class ExtendAuctionCommandHandler : ICommandHandler<ExtendAuctionCommand, DateTimeOffset>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<ExtendAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ExtendAuctionCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<ExtendAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DateTimeOffset>> Handle(ExtendAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Extending auction {AuctionId} by {Minutes} minutes",
            request.AuctionId, request.ExtensionMinutes);

        var auction = await _repository.GetByIdForUpdateAsync(request.AuctionId, cancellationToken);

        if (auction == null)
        {
            return Result.Failure<DateTimeOffset>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }

        if (auction.SellerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to extend auction {AuctionId} owned by {OwnerId}",
                request.UserId, request.AuctionId, auction.SellerId);
            return Result.Failure<DateTimeOffset>(
                Error.Create("Auction.Forbidden", "You are not authorized to extend this auction"));
        }

        if (auction.Status != Status.Live)
        {
            return Result.Failure<DateTimeOffset>(AuctionErrors.Auction.InvalidStatus(auction.Status.ToString()));
        }

        try
        {
            var extension = TimeSpan.FromMinutes(request.ExtensionMinutes);
            auction.ExtendAuctionEnd(extension);

            await _repository.UpdateAsync(auction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auction {AuctionId} extended to {NewEnd}",
                request.AuctionId, auction.AuctionEnd);

            return Result.Success(auction.AuctionEnd);
        }
        catch (InvalidEntityStateException ex)
        {
            _logger.LogWarning(ex, "Cannot extend auction {AuctionId}", request.AuctionId);
            return Result.Failure<DateTimeOffset>(AuctionErrors.Auction.InvalidStatus(ex.Message));
        }
    }
}
