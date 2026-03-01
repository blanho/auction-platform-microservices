using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auctions.Application.Commands.ExportAuctions;

public class JsonReportExporter : IReportExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ExportFormat Format => ExportFormat.Json;
    public string ContentType => "application/json";
    public string FileExtension => ".json";

    public byte[] Export(IReadOnlyList<ExportAuctionRow> records)
    {
        var report = new ExportReportEnvelope(
            Metadata: new ReportMetadata(
                GeneratedAt: DateTimeOffset.UtcNow,
                TotalRecords: records.Count,
                Format: "json"),
            Records: records);

        return JsonSerializer.SerializeToUtf8Bytes(report, SerializerOptions);
    }

    private sealed record ExportReportEnvelope(
        ReportMetadata Metadata,
        IReadOnlyList<ExportAuctionRow> Records);

    private sealed record ReportMetadata(
        DateTimeOffset GeneratedAt,
        int TotalRecords,
        string Format);
}
