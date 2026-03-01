using Auctions.Domain.Enums;

namespace Auctions.Application.Features.Auctions.ExportAuctions.Streaming;

public record ExportAuctionsStreamCommand(
    ExportFormat Format,
    Stream OutputStream,
    Status? StatusFilter = null,
    string? SellerFilter = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null,
    int BatchSize = 500) : ICommand<ExportAuctionsStreamResult>;

public record ExportAuctionsStreamResult(
    string FileName,
    string ContentType,
    int TotalRecords,
    ExportFormat Format,
    TimeSpan Duration);
