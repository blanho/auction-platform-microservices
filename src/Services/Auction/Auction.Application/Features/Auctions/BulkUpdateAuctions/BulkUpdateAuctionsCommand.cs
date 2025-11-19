namespace Auctions.Application.Commands.BulkUpdateAuctions;

public record BulkUpdateAuctionsCommand(
    List<Guid> AuctionIds,
    bool Activate,
    string? Reason = null
) : ICommand<int>;

