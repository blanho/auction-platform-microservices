using System.Globalization;
using System.Text;

namespace Auctions.Application.Commands.ExportAuctions;

public class CsvReportExporter : IReportExporter
{
    private const char Delimiter = ',';
    private static readonly string[] Headers =
    [
        "AuctionId", "Title", "Seller", "Status", "Currency",
        "ReservePrice", "CurrentHighBid", "SoldAmount",
        "CreatedAt", "AuctionEnd", "Category", "Condition"
    ];

    public ExportFormat Format => ExportFormat.Csv;
    public string ContentType => "text/csv";
    public string FileExtension => ".csv";

    public byte[] Export(IReadOnlyList<ExportAuctionRow> records)
    {
        var builder = new StringBuilder(records.Count * 200);

        builder.AppendLine(string.Join(Delimiter, Headers));

        foreach (var record in records)
        {
            builder.AppendLine(FormatRow(record));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string FormatRow(ExportAuctionRow row)
    {
        return string.Join(Delimiter,
            row.AuctionId,
            EscapeCsvField(row.Title),
            EscapeCsvField(row.Seller),
            row.Status,
            row.Currency,
            row.ReservePrice.ToString("F2", CultureInfo.InvariantCulture),
            row.CurrentHighBid?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty,
            row.SoldAmount?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty,
            row.CreatedAt.ToString("O"),
            row.AuctionEnd.ToString("O"),
            EscapeCsvField(row.Category ?? string.Empty),
            EscapeCsvField(row.Condition ?? string.Empty));
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(Delimiter) || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
