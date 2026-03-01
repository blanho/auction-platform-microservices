using System.Text;
using System.Text.Json;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;
using Auctions.Application.Features.Auctions.ExportAuctions.Streaming;
using Auctions.Application.Interfaces;

namespace Auction.Application.Tests.Features.Auctions;

public class StreamingJsonReportExporterTests
{
    private readonly StreamingJsonReportExporter _exporter = new();

    [Fact]
    public void Format_ShouldBeJson()
    {
        _exporter.Format.Should().Be(ExportFormat.Json);
    }

    [Fact]
    public void ContentType_ShouldBeApplicationJson()
    {
        _exporter.ContentType.Should().Be("application/json");
    }

    [Fact]
    public void FileExtension_ShouldBeDotJson()
    {
        _exporter.FileExtension.Should().Be(".json");
    }

    [Fact]
    public async Task ExportAsync_WithEmptyStream_ShouldProduceValidJson()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(EmptyAsyncEnumerable(), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        doc.Should().NotBeNull();
    }

    [Fact]
    public async Task ExportAsync_WithEmptyStream_ShouldContainMetadata()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(EmptyAsyncEnumerable(), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("metadata", out var metadata).Should().BeTrue();
        metadata.TryGetProperty("exportedAt", out _).Should().BeTrue();
        metadata.TryGetProperty("format", out var format).Should().BeTrue();
        format.GetString().Should().Be("json");
    }

    [Fact]
    public async Task ExportAsync_WithEmptyStream_ShouldHaveZeroTotalRecords()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(EmptyAsyncEnumerable(), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("totalRecords").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ExportAsync_WithEmptyStream_ShouldHaveEmptyRecordsArray()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(EmptyAsyncEnumerable(), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("records").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task ExportAsync_WithRecords_ShouldContainCorrectTotalRecords()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(CreateTestRecordsAsync(5), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("totalRecords").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task ExportAsync_WithRecords_ShouldContainRecordsArray()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(CreateTestRecordsAsync(3), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("records").GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task ExportAsync_ShouldWriteRecordFieldsInCamelCase()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(SingleRecordAsync(CreateTestRecord()), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        var firstRecord = doc.RootElement.GetProperty("records")[0];
        firstRecord.TryGetProperty("auctionId", out _).Should().BeTrue();
        firstRecord.TryGetProperty("title", out _).Should().BeTrue();
        firstRecord.TryGetProperty("seller", out _).Should().BeTrue();
        firstRecord.TryGetProperty("reservePrice", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsync_WithNullOptionalFields_ShouldOmitNullValues()
    {
        using var outputStream = new MemoryStream();
        var record = CreateTestRecord() with
        {
            CurrentHighBid = null,
            SoldAmount = null,
            Category = null,
            Condition = null
        };

        await _exporter.ExportAsync(SingleRecordAsync(record), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        var firstRecord = doc.RootElement.GetProperty("records")[0];
        firstRecord.TryGetProperty("currentHighBid", out _).Should().BeFalse();
        firstRecord.TryGetProperty("soldAmount", out _).Should().BeFalse();
        firstRecord.TryGetProperty("category", out _).Should().BeFalse();
        firstRecord.TryGetProperty("condition", out _).Should().BeFalse();
    }

    [Fact]
    public async Task ExportAsync_WithLargeRecordSet_ShouldProduceValidJson()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(CreateTestRecordsAsync(200), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("records").GetArrayLength().Should().Be(200);
        doc.RootElement.GetProperty("totalRecords").GetInt32().Should().Be(200);
    }

    [Fact]
    public async Task ExportAsync_ShouldLeaveOutputStreamOpen()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(CreateTestRecordsAsync(1), outputStream, CancellationToken.None);

        outputStream.CanRead.Should().BeTrue();
        outputStream.CanWrite.Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsync_ShouldSerializeDecimalsCorrectly()
    {
        using var outputStream = new MemoryStream();
        var record = CreateTestRecord() with { ReservePrice = 1234.56m };

        await _exporter.ExportAsync(SingleRecordAsync(record), outputStream, CancellationToken.None);

        var json = Encoding.UTF8.GetString(outputStream.ToArray());
        var doc = JsonDocument.Parse(json);
        var firstRecord = doc.RootElement.GetProperty("records")[0];
        firstRecord.GetProperty("reservePrice").GetDecimal().Should().Be(1234.56m);
    }

    private static ExportAuctionRow CreateTestRecord()
    {
        return new ExportAuctionRow(
            AuctionId: Guid.NewGuid(),
            Title: "Test Auction",
            Seller: "test_seller",
            Status: "Active",
            Currency: "USD",
            ReservePrice: 100.00m,
            CurrentHighBid: 150.00m,
            SoldAmount: null,
            CreatedAt: DateTimeOffset.UtcNow,
            AuctionEnd: DateTimeOffset.UtcNow.AddDays(7),
            Category: "Electronics",
            Condition: "New");
    }

    private static async IAsyncEnumerable<ExportAuctionRow> EmptyAsyncEnumerable()
    {
        await Task.CompletedTask;
        yield break;
    }

    private static async IAsyncEnumerable<ExportAuctionRow> SingleRecordAsync(ExportAuctionRow record)
    {
        await Task.CompletedTask;
        yield return record;
    }

    private static async IAsyncEnumerable<ExportAuctionRow> CreateTestRecordsAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            await Task.CompletedTask;
            yield return CreateTestRecord() with { Title = $"Auction {i + 1}" };
        }
    }
}
