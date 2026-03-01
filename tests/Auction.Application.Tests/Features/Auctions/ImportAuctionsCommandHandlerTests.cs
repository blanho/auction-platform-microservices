using Auctions.Application.Commands.ImportAuctions;
using Auctions.Application.Features.Auctions.ImportAuctions;
using Auctions.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Application.Tests.Features.Auctions;

public class ImportAuctionsCommandHandlerTests
{
    private readonly IAuctionBulkRepository _bulkRepository;
    private readonly IImportCheckpointRepository _checkpointRepository;
    private readonly ISanitizationService _sanitizationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImportAuctionsCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ImportAuctionsCommandHandler _handler;

    public ImportAuctionsCommandHandlerTests()
    {
        _bulkRepository = Substitute.For<IAuctionBulkRepository>();
        _checkpointRepository = Substitute.For<IImportCheckpointRepository>();
        _sanitizationService = Substitute.For<ISanitizationService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<ImportAuctionsCommandHandler>>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _dateTimeProvider.UtcNowOffset.Returns(DateTimeOffset.UtcNow);
        _sanitizationService.SanitizeText(Arg.Any<string?>()).Returns(x => x.Arg<string?>() ?? string.Empty);
        _sanitizationService.SanitizeHtml(Arg.Any<string?>()).Returns(x => x.Arg<string?>() ?? string.Empty);
        _bulkRepository.CountByCorrelationIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(0);
        _checkpointRepository.GetCheckpointAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ImportCheckpoint?)null);

        _handler = new ImportAuctionsCommandHandler(
            _bulkRepository,
            _checkpointRepository,
            _sanitizationService,
            _unitOfWork,
            _logger,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_WithValidRows_ShouldReturnSuccessResult()
    {
        var command = CreateValidCommand(rowCount: 3);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CorrelationId.Should().Be(command.CorrelationId);
        result.Value.TotalRows.Should().Be(3);
        result.Value.SucceededCount.Should().Be(3);
        result.Value.FailedCount.Should().Be(0);
        result.Value.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidRows_ShouldCallBulkInsertAndSaveChanges()
    {
        var command = CreateValidCommand(rowCount: 2);

        await _handler.Handle(command, CancellationToken.None);

        await _bulkRepository.Received(1).BulkInsertAsync(
            Arg.Is<IReadOnlyList<AuctionEntity>>(list => list.Count == 2),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithRowsExceedingBatchSize_ShouldProcessMultipleBatches()
    {
        var command = CreateValidCommand(rowCount: 600);

        await _handler.Handle(command, CancellationToken.None);

        await _bulkRepository.Received(2).BulkInsertAsync(
            Arg.Any<IReadOnlyList<AuctionEntity>>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingCorrelationId_ShouldReportSkippedDuplicates()
    {
        _bulkRepository.CountByCorrelationIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(5);
        var command = CreateValidCommand(rowCount: 2);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.SkippedDuplicateCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldReportValidationError()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { Title = "" }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FailedCount.Should().Be(1);
        result.Value.SucceededCount.Should().Be(0);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "Title" && e.RowNumber == 1);
    }

    [Fact]
    public async Task Handle_WithTitleExceedingMaxLength_ShouldReportValidationError()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { Title = new string('A', 201) }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FailedCount.Should().Be(1);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "Title" && e.RowNumber == 1);
    }

    [Fact]
    public async Task Handle_WithEmptyDescription_ShouldReportValidationError()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { Description = "" }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FailedCount.Should().Be(1);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "Description" && e.RowNumber == 1);
    }

    [Fact]
    public async Task Handle_WithNegativeReservePrice_ShouldReportValidationError()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { ReservePrice = -1m }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FailedCount.Should().Be(1);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "ReservePrice" && e.RowNumber == 1);
    }

    [Fact]
    public async Task Handle_WithBuyNowPriceLessThanReserve_ShouldReportValidationError()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { ReservePrice = 100m, BuyNowPrice = 50m }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FailedCount.Should().Be(1);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "BuyNowPrice" && e.RowNumber == 1);
    }

    [Fact]
    public async Task Handle_WithBuyNowPriceEqualToReserve_ShouldReportValidationError()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { ReservePrice = 100m, BuyNowPrice = 100m }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FailedCount.Should().Be(1);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "BuyNowPrice" && e.RowNumber == 1);
    }

    [Fact]
    public async Task Handle_WithPastAuctionEnd_ShouldReportValidationError()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { AuctionEnd = DateTimeOffset.UtcNow.AddDays(-1) }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FailedCount.Should().Be(1);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "AuctionEnd" && e.RowNumber == 1);
    }

    [Theory]
    [InlineData(1899)]
    [InlineData(2200)]
    public async Task Handle_WithInvalidYearManufactured_ShouldReportValidationError(int year)
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { YearManufactured = year }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.FailedCount.Should().Be(1);
        result.Value.Errors.Should().ContainSingle(e =>
            e.Field == "YearManufactured" && e.RowNumber == 1);
    }

    [Fact]
    public async Task Handle_WithMixOfValidAndInvalidRows_ShouldProcessValidAndReportInvalid()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow(),
            CreateValidRow() with { Title = "" },
            CreateValidRow(),
            CreateValidRow() with { ReservePrice = -5m }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.TotalRows.Should().Be(4);
        result.Value.SucceededCount.Should().Be(2);
        result.Value.FailedCount.Should().Be(2);
        result.Value.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithAllInvalidRows_ShouldNotCallBulkInsert()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { Title = "" },
            CreateValidRow() with { Description = "" }
        };
        var command = CreateCommandWithRows(rows);

        await _handler.Handle(command, CancellationToken.None);

        await _bulkRepository.DidNotReceive().BulkInsertAsync(
            Arg.Any<IReadOnlyList<AuctionEntity>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSanitizeTitleAndDescription()
    {
        var command = CreateValidCommand(rowCount: 1);

        await _handler.Handle(command, CancellationToken.None);

        _sanitizationService.Received().SanitizeText(Arg.Any<string?>());
        _sanitizationService.Received().SanitizeHtml(Arg.Any<string?>());
    }

    [Fact]
    public async Task Handle_ShouldReturnDurationGreaterThanZero()
    {
        var command = CreateValidCommand(rowCount: 1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_WithCheckpoint_ShouldResumeFromLastProcessedRow()
    {
        var command = CreateValidCommand(rowCount: 5);
        var checkpoint = new ImportCheckpoint(
            command.CorrelationId,
            LastProcessedRowIndex: 3,
            SucceededCount: 3,
            FailedCount: 0,
            LastUpdatedAt: DateTimeOffset.UtcNow);

        _checkpointRepository.GetCheckpointAsync(command.CorrelationId, Arg.Any<CancellationToken>())
            .Returns(checkpoint);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SucceededCount.Should().Be(5);
        await _bulkRepository.Received(1).BulkInsertAsync(
            Arg.Is<IReadOnlyList<AuctionEntity>>(list => list.Count == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSaveCheckpointAfterEachBatch()
    {
        var command = CreateValidCommand(rowCount: 600);

        await _handler.Handle(command, CancellationToken.None);

        await _checkpointRepository.Received(2).SaveCheckpointAsync(
            Arg.Any<ImportCheckpoint>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OnCompletion_ShouldDeleteCheckpoint()
    {
        var command = CreateValidCommand(rowCount: 3);

        await _handler.Handle(command, CancellationToken.None);

        await _checkpointRepository.Received(1).DeleteCheckpointAsync(
            command.CorrelationId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAllInvalidRows_ShouldDeleteCheckpoint()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { Title = "" },
            CreateValidRow() with { Description = "" }
        };
        var command = CreateCommandWithRows(rows);

        await _handler.Handle(command, CancellationToken.None);

        await _checkpointRepository.Received(1).DeleteCheckpointAsync(
            command.CorrelationId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCheckpointAtEnd_ShouldNotInsertAnyMoreRows()
    {
        var command = CreateValidCommand(rowCount: 3);
        var checkpoint = new ImportCheckpoint(
            command.CorrelationId,
            LastProcessedRowIndex: 3,
            SucceededCount: 3,
            FailedCount: 0,
            LastUpdatedAt: DateTimeOffset.UtcNow);

        _checkpointRepository.GetCheckpointAsync(command.CorrelationId, Arg.Any<CancellationToken>())
            .Returns(checkpoint);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.SucceededCount.Should().Be(3);
        await _bulkRepository.DidNotReceive().BulkInsertAsync(
            Arg.Any<IReadOnlyList<AuctionEntity>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleErrorsOnSingleRow_ShouldReportAllErrors()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { Title = "", Description = "", ReservePrice = -1m }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Errors.Should().HaveCount(3);
        result.Value.Errors.Select(e => e.Field).Should().Contain("Title");
        result.Value.Errors.Select(e => e.Field).Should().Contain("Description");
        result.Value.Errors.Select(e => e.Field).Should().Contain("ReservePrice");
    }

    [Fact]
    public async Task Handle_WithValidBuyNowPriceAboveReserve_ShouldSucceed()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { ReservePrice = 100m, BuyNowPrice = 150m }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.SucceededCount.Should().Be(1);
        result.Value.FailedCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNullBuyNowPrice_ShouldSucceed()
    {
        var rows = new List<ImportAuctionRow>
        {
            CreateValidRow() with { BuyNowPrice = null }
        };
        var command = CreateCommandWithRows(rows);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.SucceededCount.Should().Be(1);
        result.Value.FailedCount.Should().Be(0);
    }

    private static ImportAuctionsCommand CreateValidCommand(int rowCount)
    {
        var rows = Enumerable.Range(0, rowCount)
            .Select(_ => CreateValidRow())
            .ToList();

        return new ImportAuctionsCommand(
            SellerId: Guid.NewGuid(),
            SellerUsername: "test_seller",
            CorrelationId: Guid.NewGuid().ToString(),
            Currency: "USD",
            Rows: rows);
    }

    private static ImportAuctionsCommand CreateCommandWithRows(List<ImportAuctionRow> rows)
    {
        return new ImportAuctionsCommand(
            SellerId: Guid.NewGuid(),
            SellerUsername: "test_seller",
            CorrelationId: Guid.NewGuid().ToString(),
            Currency: "USD",
            Rows: rows);
    }

    private static ImportAuctionRow CreateValidRow()
    {
        return new ImportAuctionRow(
            Title: $"Test Auction {Guid.NewGuid():N}",
            Description: "A valid test description for the auction item.",
            Condition: "New",
            YearManufactured: 2024,
            ReservePrice: 100m,
            BuyNowPrice: null,
            AuctionEnd: DateTimeOffset.UtcNow.AddDays(7));
    }
}
