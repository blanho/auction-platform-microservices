using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Commands.QueueBulkUpdateAuctions;

public record QueueBulkUpdateAuctionsCommand(
    Guid RequestedBy,
    List<Guid> AuctionIds,
    bool Activate,
    string? Reason = null) : ICommand<BackgroundJobResult>;
