namespace Bidding.Application.Interfaces;

public record AuctionValidationResult(
    bool IsValid,
    string ErrorMessage = "",
    string ErrorCode = "",
    decimal ReservePrice = 0);

public record AuctionDetails(
    string Title,
    string SellerUsername,
    DateTimeOffset EndTime,
    string Status,
    bool IsReserved,
    decimal ReservePrice = 0);

public record ExtendAuctionResult(
    bool Success,
    string Message,
    DateTimeOffset? NewEndTime = null);

public interface IAuctionGrpcClient
{
    Task<AuctionValidationResult> ValidateAuctionForBidAsync(
        Guid auctionId,
        string bidderUsername,
        decimal bidAmount,
        CancellationToken cancellationToken = default);

    Task<AuctionDetails?> GetAuctionDetailsAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);

    Task<ExtendAuctionResult> ExtendAuctionAsync(
        Guid auctionId,
        DateTimeOffset newEndTime,
        CancellationToken cancellationToken = default);
}
