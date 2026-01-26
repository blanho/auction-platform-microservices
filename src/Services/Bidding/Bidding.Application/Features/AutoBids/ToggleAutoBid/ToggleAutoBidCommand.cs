namespace Bidding.Application.Features.AutoBids.ToggleAutoBid;

public record ToggleAutoBidCommand(
    Guid AutoBidId,
    Guid UserId,
    bool Activate) : ICommand<ToggleAutoBidResult>;

public record ToggleAutoBidResult
{
    public Guid AutoBidId { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? LastBidAt { get; init; }

    public static ToggleAutoBidResult From(Guid autoBidId, bool isActive, DateTimeOffset? lastBidAt) =>
        new() { AutoBidId = autoBidId, IsActive = isActive, LastBidAt = lastBidAt };
}
