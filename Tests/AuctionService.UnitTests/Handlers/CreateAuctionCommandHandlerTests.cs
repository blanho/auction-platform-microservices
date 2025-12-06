using AuctionService.Application.Commands.CreateAuction;
using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Audit.Abstractions;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;
using Moq;
using FluentAssertions;

namespace AuctionService.UnitTests.Handlers;

public class CreateAuctionCommandHandlerTests
{
    private readonly Mock<IAuctionRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IAppLogger<CreateAuctionCommandHandler>> _loggerMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuditPublisher> _auditPublisherMock;
    private readonly CreateAuctionCommandHandler _handler;

    public CreateAuctionCommandHandlerTests()
    {
        _repositoryMock = new Mock<IAuctionRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<IAppLogger<CreateAuctionCommandHandler>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _auditPublisherMock = new Mock<IAuditPublisher>();

        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        _handler = new CreateAuctionCommandHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _dateTimeProviderMock.Object,
            _eventPublisherMock.Object,
            _unitOfWorkMock.Object,
            _auditPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var command = CreateValidCommand();
        var auctionId = Guid.NewGuid();
        var createdAuction = CreateAuction(auctionId, command);
        var expectedDto = CreateAuctionDto(auctionId, command);

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAuction);

        _mapperMock
            .Setup(x => x.Map<AuctionCreatedEvent>(It.IsAny<Auction>()))
            .Returns(new AuctionCreatedEvent { Id = auctionId });

        _mapperMock
            .Setup(x => x.Map<AuctionDto>(It.IsAny<Auction>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(auctionId);
        result.Value.Title.Should().Be(command.Title);
    }

    [Fact]
    public async Task Handle_WithValidCommand_PublishesEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var auctionId = Guid.NewGuid();
        var createdAuction = CreateAuction(auctionId, command);

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAuction);

        _mapperMock
            .Setup(x => x.Map<AuctionCreatedEvent>(It.IsAny<Auction>()))
            .Returns(new AuctionCreatedEvent { Id = auctionId });

        _mapperMock
            .Setup(x => x.Map<AuctionDto>(It.IsAny<Auction>()))
            .Returns(CreateAuctionDto(auctionId, command));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _eventPublisherMock.Verify(
            x => x.PublishAsync(It.IsAny<AuctionCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_SavesChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var auctionId = Guid.NewGuid();
        var createdAuction = CreateAuction(auctionId, command);

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAuction);

        _mapperMock
            .Setup(x => x.Map<AuctionCreatedEvent>(It.IsAny<Auction>()))
            .Returns(new AuctionCreatedEvent { Id = auctionId });

        _mapperMock
            .Setup(x => x.Map<AuctionDto>(It.IsAny<Auction>()))
            .Returns(CreateAuctionDto(auctionId, command));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureResult()
    {
        // Arrange
        var command = CreateValidCommand();
        var expectedException = new InvalidOperationException("Database error");

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Code.Should().Be("Auction.CreateFailed");
    }

    [Fact]
    public async Task Handle_WithValidCommand_LogsInformation()
    {
        // Arrange
        var command = CreateValidCommand();
        var auctionId = Guid.NewGuid();
        var createdAuction = CreateAuction(auctionId, command);

        _repositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Auction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAuction);

        _mapperMock
            .Setup(x => x.Map<AuctionCreatedEvent>(It.IsAny<Auction>()))
            .Returns(new AuctionCreatedEvent { Id = auctionId });

        _mapperMock
            .Setup(x => x.Map<AuctionDto>(It.IsAny<Auction>()))
            .Returns(CreateAuctionDto(auctionId, command));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.AtLeast(1));
    }

    private static CreateAuctionCommand CreateValidCommand()
    {
        return new CreateAuctionCommand(
            Title: "2023 Tesla Model S",
            Description: "Beautiful electric vehicle in excellent condition with low mileage.",
            Make: "Tesla",
            Model: "Model S",
            Year: 2023,
            Color: "White",
            Mileage: 15000,
            ImageUrl: "https://example.com/tesla.jpg",
            ReservePrice: 50000,
            AuctionEnd: DateTimeOffset.UtcNow.AddDays(7),
            Seller: "test-seller"
        );
    }

    private static Auction CreateAuction(Guid id, CreateAuctionCommand command)
    {
        return new Auction
        {
            Id = id,
            Seller = command.Seller,
            ReversePrice = command.ReservePrice,
            AuctionEnd = command.AuctionEnd,
            Status = Status.Live,
            Item = new Item
            {
                Title = command.Title,
                Description = command.Description,
                Make = command.Make,
                Model = command.Model,
                Year = command.Year,
                Color = command.Color,
                Mileage = command.Mileage,
                ImageUrl = command.ImageUrl ?? string.Empty
            }
        };
    }

    private static AuctionDto CreateAuctionDto(Guid id, CreateAuctionCommand command)
    {
        return new AuctionDto
        {
            Id = id,
            Seller = command.Seller,
            ReservePrice = command.ReservePrice,
            AuctionEnd = command.AuctionEnd,
            Status = "Live",
            Title = command.Title,
            Description = command.Description,
            Make = command.Make,
            Model = command.Model,
            Year = command.Year,
            Color = command.Color,
            Mileage = command.Mileage,
            ImageUrl = command.ImageUrl ?? string.Empty
        };
    }
}
