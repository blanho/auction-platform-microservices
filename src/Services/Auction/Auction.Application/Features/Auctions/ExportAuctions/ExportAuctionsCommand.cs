using Auctions.Domain.Enums;

namespace Auctions.Application.Commands.ExportAuctions;

public record ExportAuctionsCommand(
    ExportFormat Format,
    Status? StatusFilter = null,
    string? SellerFilter = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null) : ICommand<ExportAuctionsResult>;

public record ExportAuctionsResult(
    byte[] Content,
    string FileName,
    string ContentType,
    int TotalRecords,
    ExportFormat Format);
