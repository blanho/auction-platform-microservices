using Auctions.Application.Commands.ExportAuctions;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;

namespace Auction.Application.Tests.Features.Auctions;

public class CsvReportExporterTests
{
    private readonly CsvReportExporter _exporter = new();

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
    public void Export_WithEmptyList_ShouldReturnHeaderOnly()
    {
        var result = _exporter.Export([]);
        var content = System.Text.Encoding.UTF8.GetString(result);

        content.Should().StartWith("AuctionId,Title,Seller,Status,Currency,");
        content.Trim().Split('\n').Should().HaveCount(1);
    }

    [Fact]
    public void Export_WithRecords_ShouldContainHeaderAndDataRows()
    {
        var records = CreateTestRecords(3);

        var result = _exporter.Export(records);
        var content = System.Text.Encoding.UTF8.GetString(result);
        var lines = content.Trim().Split('\n');

        lines.Should().HaveCount(4);
        lines[0].Should().StartWith("AuctionId,Title,Seller,Status,Currency,");
    }

    [Fact]
    public void Export_ShouldContainAllHeaders()
    {
        var result = _exporter.Export([]);
        var content = System.Text.Encoding.UTF8.GetString(result);
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
    public void Export_WithCommaInTitle_ShouldEscapeField()
    {
        var records = new List<ExportAuctionRow>
        {
            CreateRow(title: "Vintage, Rare Watch")
        };

        var result = _exporter.Export(records);
        var content = System.Text.Encoding.UTF8.GetString(result);

        content.Should().Contain("\"Vintage, Rare Watch\"");
    }

    [Fact]
    public void Export_WithQuotesInTitle_ShouldDoubleQuotesAndWrap()
    {
        var records = new List<ExportAuctionRow>
        {
            CreateRow(title: "The \"Best\" Watch")
        };

        var result = _exporter.Export(records);
        var content = System.Text.Encoding.UTF8.GetString(result);

        content.Should().Contain("\"The \"\"Best\"\" Watch\"");
    }

    [Fact]
    public void Export_WithNewlineInTitle_ShouldEscapeField()
    {
        var records = new List<ExportAuctionRow>
        {
            CreateRow(title: "Watch\nDescription")
        };

        var result = _exporter.Export(records);
        var content = System.Text.Encoding.UTF8.GetString(result);

        content.Should().Contain("\"Watch\nDescription\"");
    }

    [Fact]
    public void Export_WithNullOptionalFields_ShouldOutputEmptyStrings()
    {
        var records = new List<ExportAuctionRow>
        {
            CreateRow(currentHighBid: null, soldAmount: null, category: null, condition: null)
        };

        var result = _exporter.Export(records);
        var content = System.Text.Encoding.UTF8.GetString(result);
        var dataLine = content.Trim().Split('\n')[1];

        dataLine.Should().Contain(",,");
    }

    [Fact]
    public void Export_ShouldFormatPricesWithTwoDecimalPlaces()
    {
        var records = new List<ExportAuctionRow>
        {
            CreateRow(reservePrice: 100m, currentHighBid: 150.5m)
        };

        var result = _exporter.Export(records);
        var content = System.Text.Encoding.UTF8.GetString(result);

        content.Should().Contain("100.00");
        content.Should().Contain("150.50");
    }

    [Fact]
    public void Export_ShouldReturnNonEmptyByteArray()
    {
        var records = CreateTestRecords(1);

        var result = _exporter.Export(records);

        result.Should().NotBeEmpty();
    }

    private static List<ExportAuctionRow> CreateTestRecords(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateRow())
            .ToList();
    }

    private static ExportAuctionRow CreateRow(
        string title = "Test Auction",
        string seller = "test_seller",
        string status = "Live",
        string currency = "USD",
        decimal reservePrice = 100m,
        decimal? currentHighBid = 150m,
        decimal? soldAmount = null,
        string? category = "Electronics",
        string? condition = "New")
    {
        return new ExportAuctionRow(
            AuctionId: Guid.NewGuid(),
            Title: title,
            Seller: seller,
            Status: status,
            Currency: currency,
            ReservePrice: reservePrice,
            CurrentHighBid: currentHighBid,
            SoldAmount: soldAmount,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-5),
            AuctionEnd: DateTimeOffset.UtcNow.AddDays(2),
            Category: category,
            Condition: condition);
    }
}
