using Auctions.Domain.Enums;
using BuildingBlocks.Application.CQRS;

namespace Auctions.Application.Commands.QueueAuctionExport;

public record QueueAuctionExportCommand(
    Guid RequestedBy,
    ExportFormat Format,
    Status? StatusFilter = null,
    string? SellerFilter = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null) : ICommand<BackgroundJobResult>;
