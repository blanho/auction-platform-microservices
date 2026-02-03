using Bidding.Application.DTOs;
using Bidding.Application.Features.Bids.Commands.PlaceBid;
using Bidding.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;

namespace Bidding.Application.Tests.Features.Bids;

public class PlaceBidCommandHandlerTests
{
    private readonly IBidService _bidService;
    private readonly PlaceBidCommandHandler _handler;

    public PlaceBidCommandHandlerTests()
    {
        _bidService = Substitute.For<IBidService>();
        _handler = new PlaceBidCommandHandler(_bidService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPlaceBid()
    {
        var command = CreateValidCommand();
        var expectedBidDto = new BidDto
        {
            Id = Guid.NewGuid(),
            AuctionId = command.AuctionId,
            Amount = command.Amount,
            BidderId = command.BidderId,
            BidderUsername = command.BidderUsername
        };

        _bidService.PlaceBidAsync(
            Arg.Any<PlaceBidDto>(),
            command.BidderId,
            command.BidderUsername,
            Arg.Any<CancellationToken>())
            .Returns(expectedBidDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AuctionId.Should().Be(command.AuctionId);
        result.Value.Amount.Should().Be(command.Amount);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsError_ShouldReturnFailure()
    {
        var command = CreateValidCommand();
        var errorDto = new BidDto
        {
            ErrorMessage = "Bid amount is too low"
        };

        _bidService.PlaceBidAsync(
            Arg.Any<PlaceBidDto>(),
            command.BidderId,
            command.BidderUsername,
            Arg.Any<CancellationToken>())
            .Returns(errorDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("Bid.PlaceFailed");
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToService()
    {
        var command = CreateValidCommand();
        PlaceBidDto? capturedDto = null;

        _bidService.PlaceBidAsync(
            Arg.Do<PlaceBidDto>(dto => capturedDto = dto),
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new BidDto());

        await _handler.Handle(command, CancellationToken.None);

        capturedDto.Should().NotBeNull();
        capturedDto!.AuctionId.Should().Be(command.AuctionId);
        capturedDto.Amount.Should().Be(command.Amount);
    }

    [Fact]
    public async Task Handle_ShouldPassBidderInfoToService()
    {
        var command = CreateValidCommand();
        Guid capturedBidderId = Guid.Empty;
        string capturedBidderUsername = string.Empty;

        _bidService.PlaceBidAsync(
            Arg.Any<PlaceBidDto>(),
            Arg.Do<Guid>(id => capturedBidderId = id),
            Arg.Do<string>(username => capturedBidderUsername = username),
            Arg.Any<CancellationToken>())
            .Returns(new BidDto());

        await _handler.Handle(command, CancellationToken.None);

        capturedBidderId.Should().Be(command.BidderId);
        capturedBidderUsername.Should().Be(command.BidderUsername);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(100)]
    [InlineData(10000)]
    public async Task Handle_WithVariousAmounts_ShouldPassAmountToService(decimal amount)
    {
        var command = CreateValidCommand() with { Amount = amount };
        PlaceBidDto? capturedDto = null;

        _bidService.PlaceBidAsync(
            Arg.Do<PlaceBidDto>(dto => capturedDto = dto),
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns(new BidDto());

        await _handler.Handle(command, CancellationToken.None);

        capturedDto!.Amount.Should().Be(amount);
    }

    private static PlaceBidCommand CreateValidCommand()
    {
        return new PlaceBidCommand(
            AuctionId: Guid.NewGuid(),
            Amount: 100m,
            BidderId: Guid.NewGuid(),
            BidderUsername: "test_bidder",
            IdempotencyKey: Guid.NewGuid().ToString());
    }
}
