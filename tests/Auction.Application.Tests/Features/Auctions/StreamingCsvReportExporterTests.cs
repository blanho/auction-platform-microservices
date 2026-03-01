using System.Text;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;
using Auctions.Application.Features.Auctions.ExportAuctions.Streaming;
using Auctions.Application.Interfaces;

namespace Auction.Application.Tests.Features.Auctions;

public class StreamingCsvReportExporterTests
{
    private readonly StreamingCsvReportExporter _exporter = new();

    [Fact]
    public void Format_ShouldBeCsv()
    {
        _exporter.Format.Should().Be(ExportFormat.Csv);
    }

    [Fact]
    public void ContentType_ShouldBeTextCsv()
    {
        _exporter.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public void FileExtension_ShouldBeDotCsv()
    {
        _exporter.FileExtension.Should().Be(".csv");
    }

    [Fact]
    public async Task ExportAsync_WithEmptyStream_ShouldWriteHeaderOnly()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(EmptyAsyncEnumerable(), outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        content.Should().StartWith("AuctionId,Title,Seller,Status,Currency,");
        content.Trim().Split('\n').Should().HaveCount(1);
    }

    [Fact]
    public async Task ExportAsync_WithRecords_ShouldContainHeaderAndDataRows()
    {
        using var outputStream = new MemoryStream();
        var records = CreateTestRecordsAsync(3);

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        var lines = content.Trim().Split('\n');
        lines.Should().HaveCount(4);
    }

    [Fact]
    public async Task ExportAsync_ShouldContainAllHeaders()
    {
        using var outputStream = new MemoryStream();

        await _exporter.ExportAsync(EmptyAsyncEnumerable(), outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        var headerLine = content.Trim().Split('\n')[0];
        var expectedHeaders = new[]
        {
            "AuctionId", "Title", "Seller", "Status", "Currency",
            "ReservePrice", "CurrentHighBid", "SoldAmount",
            "CreatedAt", "AuctionEnd", "Category", "Condition"
        };

        foreach (var header in expectedHeaders)
        {
            headerLine.Should().Contain(header);
        }
    }

    [Fact]
    public async Task ExportAsync_WithCommaInTitle_ShouldEscapeField()
    {
        using var outputStream = new MemoryStream();
        var records = SingleRecordAsync(CreateTestRecord() with { Title = "Item, with comma" });

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        content.Should().Contain("\"Item, with comma\"");
    }

    [Fact]
    public async Task ExportAsync_WithQuoteInTitle_ShouldDoubleQuoteEscape()
    {
        using var outputStream = new MemoryStream();
        var records = SingleRecordAsync(CreateTestRecord() with { Title = "Item \"quoted\"" });

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        content.Should().Contain("\"Item \"\"quoted\"\"\"");
    }

    [Fact]
    public async Task ExportAsync_WithNewlineInTitle_ShouldEscapeField()
    {
        using var outputStream = new MemoryStream();
        var records = SingleRecordAsync(CreateTestRecord() with { Title = "Item\nwith newline" });

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        content.Should().Contain("\"Item\nwith newline\"");
    }

    [Fact]
    public async Task ExportAsync_WithNullOptionalFields_ShouldWriteEmptyValues()
    {
        using var outputStream = new MemoryStream();
        var record = CreateTestRecord() with
        {
            CurrentHighBid = null,
            SoldAmount = null,
            Category = null,
            Condition = null
        };
        var records = SingleRecordAsync(record);

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        var dataLine = content.Trim().Split('\n')[1];
        dataLine.Should().NotContain("null");
    }

    [Fact]
    public async Task ExportAsync_ShouldFormatDecimalsWithInvariantCulture()
    {
        using var outputStream = new MemoryStream();
        var record = CreateTestRecord() with { ReservePrice = 1234.56m, CurrentHighBid = 7890.12m };
        var records = SingleRecordAsync(record);

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        content.Should().Contain("1234.56");
        content.Should().Contain("7890.12");
    }

    [Fact]
    public async Task ExportAsync_WithLargeRecordSet_ShouldWriteAllRecords()
    {
        using var outputStream = new MemoryStream();
        var records = CreateTestRecordsAsync(250);

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        var content = Encoding.UTF8.GetString(outputStream.ToArray());
        var lines = content.Trim().Split('\n');
        lines.Should().HaveCount(251);
    }

    [Fact]
    public async Task ExportAsync_ShouldLeaveOutputStreamOpen()
    {
        using var outputStream = new MemoryStream();
        var records = CreateTestRecordsAsync(1);

        await _exporter.ExportAsync(records, outputStream, CancellationToken.None);

        outputStream.CanRead.Should().BeTrue();
        outputStream.CanWrite.Should().BeTrue();
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
