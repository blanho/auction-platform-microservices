namespace Bidding.Application.Features.AutoBids.CancelAutoBid;

public record CancelAutoBidCommand(
    Guid AutoBidId,
    Guid UserId) : ICommand<CancelAutoBidResult>;

public record CancelAutoBidResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static CancelAutoBidResult Succeeded() => new() { Success = true };
    public static CancelAutoBidResult Failed(string error) => new() { Success = false, ErrorMessage = error };
}
