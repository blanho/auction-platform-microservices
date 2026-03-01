using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;

namespace Auctions.Application.Interfaces;

public interface IStreamingReportExporter
{
    ExportFormat Format { get; }
    string ContentType { get; }
    string FileExtension { get; }

    Task ExportAsync(
        IAsyncEnumerable<ExportAuctionRow> records,
        Stream outputStream,
        CancellationToken cancellationToken = default);
}
