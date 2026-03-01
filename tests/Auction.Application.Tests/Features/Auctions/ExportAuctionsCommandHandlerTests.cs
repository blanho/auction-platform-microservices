using Auctions.Application.Commands.ExportAuctions;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;
using Auctions.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using Auctions.Domain.Enums;
using Microsoft.Extensions.Logging;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Application.Tests.Features.Auctions;

public class ExportAuctionsCommandHandlerTests
{
    private readonly IAuctionExportRepository _exportRepository;
    private readonly ILogger<ExportAuctionsCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IReportExporter _csvExporter;
    private readonly IReportExporter _jsonExporter;
    private readonly ExportAuctionsCommandHandler _handler;
    private readonly DateTime _fixedUtcNow = new(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);

    public ExportAuctionsCommandHandlerTests()
    {
        _exportRepository = Substitute.For<IAuctionExportRepository>();
        _logger = Substitute.For<ILogger<ExportAuctionsCommandHandler>>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _csvExporter = Substitute.For<IReportExporter>();
        _csvExporter.Format.Returns(ExportFormat.Csv);
        _csvExporter.ContentType.Returns("text/csv");
        _csvExporter.FileExtension.Returns(".csv");
        _csvExporter.Export(Arg.Any<IReadOnlyList<ExportAuctionRow>>())
            .Returns([0x43, 0x53, 0x56]);

        _jsonExporter = Substitute.For<IReportExporter>();
        _jsonExporter.Format.Returns(ExportFormat.Json);
        _jsonExporter.ContentType.Returns("application/json");
        _jsonExporter.FileExtension.Returns(".json");
        _jsonExporter.Export(Arg.Any<IReadOnlyList<ExportAuctionRow>>())
            .Returns([0x4A, 0x53, 0x4F, 0x4E]);

        _dateTimeProvider.UtcNow.Returns(_fixedUtcNow);

        _exportRepository.GetAuctionsForExportAsync(
                Arg.Any<Status?>(),
                Arg.Any<string?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<AuctionEntity>());

        IEnumerable<IReportExporter> exporters = [_csvExporter, _jsonExporter];

        _handler = new ExportAuctionsCommandHandler(
            _exportRepository,
            exporters,
            _logger,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WithCsvFormat_ShouldReturnCsvResult()
    {
        var command = new ExportAuctionsCommand(ExportFormat.Csv);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Format.Should().Be(ExportFormat.Csv);
        result.Value.ContentType.Should().Be("text/csv");
        result.Value.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithJsonFormat_ShouldReturnJsonResult()
    {
        var command = new ExportAuctionsCommand(ExportFormat.Json);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Format.Should().Be(ExportFormat.Json);
        result.Value.ContentType.Should().Be("application/json");
        result.Value.Content.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldGenerateCorrectFileName()
    {
        var command = new ExportAuctionsCommand(ExportFormat.Csv);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FileName.Should().Be("auctions-export-20250615-103000.csv");
    }

    [Fact]
    public async Task Handle_WithJsonFormat_ShouldGenerateJsonFileName()
    {
        var command = new ExportAuctionsCommand(ExportFormat.Json);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FileName.Should().Be("auctions-export-20250615-103000.json");
    }

    [Fact]
    public async Task Handle_WithUnsupportedFormat_ShouldReturnFailure()
    {
        IEnumerable<IReportExporter> emptyExporters = [];
        var handler = new ExportAuctionsCommandHandler(
            _exportRepository,
            emptyExporters,
            _logger,
            _dateTimeProvider);
        var command = new ExportAuctionsCommand(ExportFormat.Csv);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Export.UnsupportedFormat");
    }

    [Fact]
    public async Task Handle_ShouldPassFiltersToRepository()
    {
        var statusFilter = Status.Live;
        var sellerFilter = "test_seller";
        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;
        var command = new ExportAuctionsCommand(
            ExportFormat.Csv,
            StatusFilter: statusFilter,
            SellerFilter: sellerFilter,
            StartDate: startDate,
            EndDate: endDate);

        await _handler.Handle(command, CancellationToken.None);

        await _exportRepository.Received(1).GetAuctionsForExportAsync(
            statusFilter,
            sellerFilter,
            startDate,
            endDate,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallExporterWithMappedRows()
    {
        var command = new ExportAuctionsCommand(ExportFormat.Csv);

        await _handler.Handle(command, CancellationToken.None);

        _csvExporter.Received(1).Export(Arg.Any<IReadOnlyList<ExportAuctionRow>>());
    }

    [Fact]
    public async Task Handle_WithEmptyAuctionList_ShouldReturnZeroRecords()
    {
        var command = new ExportAuctionsCommand(ExportFormat.Csv);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.TotalRecords.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNullFilters_ShouldPassNullsToRepository()
    {
        var command = new ExportAuctionsCommand(ExportFormat.Json);

        await _handler.Handle(command, CancellationToken.None);

        await _exportRepository.Received(1).GetAuctionsForExportAsync(
            null,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }
}
