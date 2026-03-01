using Auctions.Application.Commands.ExportAuctions;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Enums;
using Auctions.Application.Features.Auctions.ExportAuctions.Streaming;
using Auctions.Application.Interfaces;
using Auctions.Domain.Enums;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Application.Tests.Features.Auctions;

public class ExportAuctionsStreamHandlerTests
{
    private readonly IAuctionStreamingExportRepository _streamingRepository;
    private readonly IStreamingReportExporter _csvExporter;
    private readonly ILogger<ExportAuctionsStreamHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ExportAuctionsStreamHandler _handler;

    public ExportAuctionsStreamHandlerTests()
    {
        _streamingRepository = Substitute.For<IAuctionStreamingExportRepository>();
        _csvExporter = Substitute.For<IStreamingReportExporter>();
        _logger = Substitute.For<ILogger<ExportAuctionsStreamHandler>>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _dateTimeProvider.UtcNow.Returns(new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc));

        _csvExporter.Format.Returns(ExportFormat.Csv);
        _csvExporter.ContentType.Returns("text/csv");
        _csvExporter.FileExtension.Returns(".csv");

        _handler = new ExportAuctionsStreamHandler(
            _streamingRepository,
            new[] { _csvExporter },
            _logger,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WithUnsupportedFormat_ShouldReturnFailure()
    {
        using var outputStream = new MemoryStream();
        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Json,
            OutputStream: outputStream);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Export.UnsupportedFormat");
    }

    [Fact]
    public async Task Handle_WithSupportedFormat_ShouldReturnSuccess()
    {
        using var outputStream = new MemoryStream();
        ConfigureEmptyStream();

        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Csv,
            OutputStream: outputStream);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectContentType()
    {
        using var outputStream = new MemoryStream();
        ConfigureEmptyStream();

        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Csv,
            OutputStream: outputStream);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectFormat()
    {
        using var outputStream = new MemoryStream();
        ConfigureEmptyStream();

        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Csv,
            OutputStream: outputStream);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Format.Should().Be(ExportFormat.Csv);
    }

    [Fact]
    public async Task Handle_ShouldReturnFileNameWithTimestamp()
    {
        using var outputStream = new MemoryStream();
        ConfigureEmptyStream();

        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Csv,
            OutputStream: outputStream);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FileName.Should().Contain("20250615-103000");
        result.Value.FileName.Should().EndWith(".csv");
    }

    [Fact]
    public async Task Handle_ShouldReturnDurationGreaterThanZero()
    {
        using var outputStream = new MemoryStream();
        ConfigureEmptyStream();

        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Csv,
            OutputStream: outputStream);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Duration.Should().BeGreaterOrEqualTo(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_ShouldPassFiltersToRepository()
    {
        using var outputStream = new MemoryStream();
        ConfigureEmptyStream();

        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Csv,
            OutputStream: outputStream,
            StatusFilter: Status.Live,
            SellerFilter: "test_seller",
            StartDate: DateTimeOffset.UtcNow.AddDays(-30),
            EndDate: DateTimeOffset.UtcNow,
            BatchSize: 100);

        await _handler.Handle(command, CancellationToken.None);

        _streamingRepository.Received(1).StreamAuctionsForExportAsync(
            Status.Live,
            "test_seller",
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<DateTimeOffset?>(),
            100,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallExporterWithOutputStream()
    {
        using var outputStream = new MemoryStream();
        ConfigureEmptyStream();

        var command = new ExportAuctionsStreamCommand(
            Format: ExportFormat.Csv,
            OutputStream: outputStream);

        await _handler.Handle(command, CancellationToken.None);

        await _csvExporter.Received(1).ExportAsync(
            Arg.Any<IAsyncEnumerable<ExportAuctionRow>>(),
            outputStream,
            Arg.Any<CancellationToken>());
    }

    private void ConfigureEmptyStream()
    {
        _streamingRepository.StreamAuctionsForExportAsync(
                Arg.Any<Status?>(),
                Arg.Any<string?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<DateTimeOffset?>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(EmptyAuctionStream());
    }

    private static async IAsyncEnumerable<AuctionEntity> EmptyAuctionStream()
    {
        await Task.CompletedTask;
        yield break;
    }
}
