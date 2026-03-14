namespace Auctions.Application.Features.Auctions.BulkUpdateAuctions;

public record BulkUpdateAuctionsCommand(
    List<Guid> AuctionIds,
    bool Activate,
    string? Reason = null
) : ICommand<int>;

