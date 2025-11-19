using Bidding.Application.DTOs;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.CQRS;

namespace Bidding.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandHandler : ICommandHandler<PlaceBidCommand, BidDto>
{
    private readonly IBidService _bidService;

    public PlaceBidCommandHandler(
        IBidService bidService)
    {
        _bidService = bidService;
    }

    public async Task<Result<BidDto>> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var dto = new PlaceBidDto
        {
            AuctionId = request.AuctionId,
            Amount = request.Amount
        };

        var result = await _bidService.PlaceBidAsync(
            dto,
            request.BidderId,
            request.BidderUsername,
            cancellationToken);

        if (!string.IsNullOrEmpty(result.ErrorMessage))
        {
            return Result.Failure<BidDto>(Error.Create("Bid.PlaceFailed", result.ErrorMessage));
        }

        return Result<BidDto>.Success(result);
    }
}
