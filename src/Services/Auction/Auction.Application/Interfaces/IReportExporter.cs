using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;

namespace Auctions.Application.Interfaces;

public interface IReportExporter
{
    ExportFormat Format { get; }
    string ContentType { get; }
    string FileExtension { get; }
    byte[] Export(IReadOnlyList<ExportAuctionRow> records);
}
