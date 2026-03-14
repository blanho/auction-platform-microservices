using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Features.Auctions.QueueBulkUpdateAuctions;

public record QueueBulkUpdateAuctionsCommand(
    Guid RequestedBy,
    List<Guid> AuctionIds,
    bool Activate,
    string? Reason = null) : ICommand<BackgroundJobResult>;
