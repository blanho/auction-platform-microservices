namespace Bidding.Application.Interfaces;

public interface IAuctionBidValidator
{
    Task<AuctionValidationResult> ValidateAsync(PlaceBidDto request, Guid bidderId, string bidderUsername,
        CancellationToken cancellationToken = default);
}
