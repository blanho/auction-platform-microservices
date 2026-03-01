using System.Text.Json;
using Auctions.Application.Commands.ExportAuctions;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;

namespace Auction.Application.Tests.Features.Auctions;

public class JsonReportExporterTests
{
    private readonly JsonReportExporter _exporter = new();

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
    public void Export_WithRecords_ShouldReturnValidJson()
    {
        var records = CreateTestRecords(2);

        var result = _exporter.Export(records);
        var json = System.Text.Encoding.UTF8.GetString(result);

        var act = () => JsonDocument.Parse(json);
        act.Should().NotThrow();
    }

    [Fact]
    public void Export_ShouldContainMetadataWithTotalRecords()
    {
        var records = CreateTestRecords(3);

        var result = _exporter.Export(records);
        var doc = JsonDocument.Parse(result);

        doc.RootElement.GetProperty("metadata").GetProperty("totalRecords").GetInt32()
            .Should().Be(3);
    }

    [Fact]
    public void Export_ShouldContainMetadataWithFormatJson()
    {
        var records = CreateTestRecords(1);

        var result = _exporter.Export(records);
        var doc = JsonDocument.Parse(result);

        doc.RootElement.GetProperty("metadata").GetProperty("format").GetString()
            .Should().Be("json");
    }

    [Fact]
    public void Export_ShouldContainMetadataWithGeneratedAt()
    {
        var records = CreateTestRecords(1);

        var result = _exporter.Export(records);
        var doc = JsonDocument.Parse(result);

        doc.RootElement.GetProperty("metadata").TryGetProperty("generatedAt", out _)
            .Should().BeTrue();
    }

    [Fact]
    public void Export_ShouldContainRecordsArray()
    {
        var records = CreateTestRecords(2);

        var result = _exporter.Export(records);
        var doc = JsonDocument.Parse(result);

        doc.RootElement.GetProperty("records").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public void Export_WithEmptyList_ShouldReturnEmptyRecordsArray()
    {
        var result = _exporter.Export([]);
        var doc = JsonDocument.Parse(result);

        doc.RootElement.GetProperty("records").GetArrayLength().Should().Be(0);
        doc.RootElement.GetProperty("metadata").GetProperty("totalRecords").GetInt32()
            .Should().Be(0);
    }

    [Fact]
    public void Export_ShouldUseCamelCasePropertyNames()
    {
        var records = CreateTestRecords(1);

        var result = _exporter.Export(records);
        var json = System.Text.Encoding.UTF8.GetString(result);

        json.Should().Contain("\"auctionId\"");
        json.Should().Contain("\"reservePrice\"");
        json.Should().Contain("\"currentHighBid\"");
    }

    [Fact]
    public void Export_WithNullFields_ShouldOmitNullValues()
    {
        var records = new List<ExportAuctionRow>
        {
            new(
                AuctionId: Guid.NewGuid(),
                Title: "Test",
                Seller: "seller",
                Status: "Live",
                Currency: "USD",
                ReservePrice: 100m,
                CurrentHighBid: null,
                SoldAmount: null,
                CreatedAt: DateTimeOffset.UtcNow,
                AuctionEnd: DateTimeOffset.UtcNow.AddDays(1),
                Category: null,
                Condition: null)
        };

        var result = _exporter.Export(records);
        var json = System.Text.Encoding.UTF8.GetString(result);

        json.Should().NotContain("\"soldAmount\"");
        json.Should().NotContain("\"category\"");
        json.Should().NotContain("\"condition\"");
    }

    [Fact]
    public void Export_ShouldReturnNonEmptyByteArray()
    {
        var records = CreateTestRecords(1);

        var result = _exporter.Export(records);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void Export_WithWrittenIndented_ShouldContainNewlines()
    {
        var records = CreateTestRecords(1);

        var result = _exporter.Export(records);
        var json = System.Text.Encoding.UTF8.GetString(result);

        json.Should().Contain("\n");
    }

    private static List<ExportAuctionRow> CreateTestRecords(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => new ExportAuctionRow(
                AuctionId: Guid.NewGuid(),
                Title: $"Test Auction {i}",
                Seller: "test_seller",
                Status: "Live",
                Currency: "USD",
                ReservePrice: 100m + i,
                CurrentHighBid: 150m + i,
                SoldAmount: null,
                CreatedAt: DateTimeOffset.UtcNow.AddDays(-5),
                AuctionEnd: DateTimeOffset.UtcNow.AddDays(2),
                Category: "Electronics",
                Condition: "New"))
            .ToList();
    }
}
