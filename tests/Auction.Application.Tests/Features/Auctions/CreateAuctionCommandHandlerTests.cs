using Auctions.Application.Commands.CreateAuction;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Application.Tests.Features.Auctions;

public class CreateAuctionCommandHandlerTests
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISanitizationService _sanitizationService;
    private readonly CreateAuctionCommandHandler _handler;

    public CreateAuctionCommandHandlerTests()
    {
        _repository = Substitute.For<IAuctionRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CreateAuctionCommandHandler>>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sanitizationService = Substitute.For<ISanitizationService>();

        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);
        _sanitizationService.SanitizeText(Arg.Any<string?>()).Returns(x => x.Arg<string?>() ?? string.Empty);
        _sanitizationService.SanitizeHtml(Arg.Any<string?>()).Returns(x => x.Arg<string?>() ?? string.Empty);

        _handler = new CreateAuctionCommandHandler(
            _repository,
            _mapper,
            _logger,
            _dateTimeProvider,
            _unitOfWork,
            _sanitizationService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAuction()
    {
        var command = CreateValidCommand();
        var expectedDto = CreateValidDto(command);

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(expectedDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        await _repository.Received(1).CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSanitizeTitleAndDescription()
    {
        var command = CreateValidCommand();

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(CreateValidDto(command));

        await _handler.Handle(command, CancellationToken.None);

        _sanitizationService.Received(1).SanitizeText(command.Title);
        _sanitizationService.Received(1).SanitizeHtml(command.Description);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryCreate()
    {
        var command = CreateValidCommand();

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(CreateValidDto(command));

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).CreateAsync(
            Arg.Is<AuctionEntity>(a => 
                a.ReservePrice == command.ReservePrice &&
                a.SellerUsername == command.SellerUsername),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var command = CreateValidCommand();

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(CreateValidDto(command));

        await _handler.Handle(command, CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapResultToDto()
    {
        var command = CreateValidCommand();
        var expectedDto = CreateValidDto(command);

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(expectedDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.Should().Be(expectedDto);
        _mapper.Received(1).Map<AuctionDto>(Arg.Any<AuctionEntity>());
    }

    [Fact]
    public async Task Handle_WithBuyNowPrice_ShouldPassToEntity()
    {
        var command = CreateValidCommand() with { BuyNowPrice = 500m };

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(CreateValidDto(command));

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).CreateAsync(
            Arg.Is<AuctionEntity>(a => a.BuyNowPrice == 500m),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithFeatured_ShouldSetFeaturedFlag()
    {
        var command = CreateValidCommand() with { IsFeatured = true };

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(CreateValidDto(command));

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).CreateAsync(
            Arg.Is<AuctionEntity>(a => a.IsFeatured == true),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public async Task Handle_WithDifferentCurrencies_ShouldSetCurrency(string currency)
    {
        var command = CreateValidCommand() with { Currency = currency };

        _repository.CreateAsync(Arg.Any<AuctionEntity>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<AuctionEntity>());
        _mapper.Map<AuctionDto>(Arg.Any<AuctionEntity>())
            .Returns(CreateValidDto(command));

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).CreateAsync(
            Arg.Is<AuctionEntity>(a => a.Currency == currency),
            Arg.Any<CancellationToken>());
    }

    private static CreateAuctionCommand CreateValidCommand()
    {
        return new CreateAuctionCommand(
            Title: "Test Auction",
            Description: "Test Description",
            Condition: "New",
            YearManufactured: 2024,
            Attributes: null,
            ReservePrice: 100m,
            BuyNowPrice: null,
            AuctionEnd: DateTimeOffset.UtcNow.AddDays(7),
            SellerId: Guid.NewGuid(),
            SellerUsername: "test_seller",
            Currency: "USD",
            Files: null,
            CategoryId: null,
            BrandId: null,
            IsFeatured: false);
    }

    private static AuctionDto CreateValidDto(CreateAuctionCommand command)
    {
        return new AuctionDto
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            Seller = command.SellerUsername,
            Status = "Live",
            ReservePrice = command.ReservePrice,
            Currency = command.Currency
        };
    }
}
