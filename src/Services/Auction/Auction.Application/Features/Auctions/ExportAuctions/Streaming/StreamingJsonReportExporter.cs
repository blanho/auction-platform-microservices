using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auctions.Application.Features.Auctions.ExportAuctions.Streaming;

public sealed class StreamingJsonReportExporter : IStreamingReportExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ExportFormat Format => ExportFormat.Json;
    public string ContentType => "application/json";
    public string FileExtension => ".json";

    public async Task ExportAsync(
        IAsyncEnumerable<ExportAuctionRow> records,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        await using var writer = new Utf8JsonWriter(outputStream, new JsonWriterOptions
        {
            Indented = true,
            SkipValidation = false
        });

        writer.WriteStartObject();

        writer.WritePropertyName("metadata");
        writer.WriteStartObject();
        writer.WriteString("exportedAt", DateTimeOffset.UtcNow);
        writer.WriteString("format", "json");
        writer.WriteEndObject();

        writer.WritePropertyName("records");
        writer.WriteStartArray();

        var recordCount = 0;

        await foreach (var record in records.WithCancellation(cancellationToken))
        {
            JsonSerializer.Serialize(writer, record, SerializerOptions);
            recordCount++;

            if (recordCount % 100 == 0)
            {
                await writer.FlushAsync(cancellationToken);
            }
        }

        writer.WriteEndArray();

        writer.WriteNumber("totalRecords", recordCount);

        writer.WriteEndObject();

        await writer.FlushAsync(cancellationToken);
    }
}
