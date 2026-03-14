namespace Bidding.Application.Interfaces;

public record AuctionValidationResult(
    bool IsValid,
    string ErrorMessage = "",
    string ErrorCode = "");

public record AuctionDetails(
    string Title,
    string SellerUsername,
    DateTime EndTime,
    string Status,
    bool IsReserved);

public record ExtendAuctionResult(
    bool Success,
    string Message,
    DateTime? NewEndTime = null);

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
        DateTime newEndTime,
        CancellationToken cancellationToken = default);
}
