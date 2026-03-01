using System.Globalization;
using System.Text;

namespace Auctions.Application.Features.Auctions.ExportAuctions.Streaming;

public sealed class StreamingCsvReportExporter : IStreamingReportExporter
{
    private const char Delimiter = ',';
    private const int FlushIntervalRows = 100;
    private static readonly string[] Headers =
    [
        "AuctionId", "Title", "Seller", "Status", "Currency",
        "ReservePrice", "CurrentHighBid", "SoldAmount",
        "CreatedAt", "AuctionEnd", "Category", "Condition"
    ];

    public ExportFormat Format => ExportFormat.Csv;
    public string ContentType => "text/csv";
    public string FileExtension => ".csv";

    public async Task ExportAsync(
        IAsyncEnumerable<ExportAuctionRow> records,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(outputStream, new UTF8Encoding(false), bufferSize: 8192, leaveOpen: true);

        await writer.WriteLineAsync(string.Join(Delimiter, Headers));

        var rowCount = 0;

        await foreach (var record in records.WithCancellation(cancellationToken))
        {
            await writer.WriteLineAsync(FormatRow(record));
            rowCount++;

            if (rowCount % FlushIntervalRows == 0)
            {
                await writer.FlushAsync(cancellationToken);
            }
        }

        await writer.FlushAsync(cancellationToken);
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
