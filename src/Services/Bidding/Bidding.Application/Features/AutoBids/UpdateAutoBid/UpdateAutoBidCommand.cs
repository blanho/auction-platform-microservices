namespace Bidding.Application.Features.AutoBids.UpdateAutoBid;

public record UpdateAutoBidCommand(
    Guid AutoBidId,
    Guid UserId,
    decimal NewMaxAmount) : ICommand<UpdateAutoBidResult>;

public record UpdateAutoBidResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public AutoBidDto? AutoBid { get; init; }

    public static UpdateAutoBidResult Succeeded(AutoBidDto autoBid) => new()
    {
        Success = true,
        AutoBid = autoBid
    };

    public static UpdateAutoBidResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
