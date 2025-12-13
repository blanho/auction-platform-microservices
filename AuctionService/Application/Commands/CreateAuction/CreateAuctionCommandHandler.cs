using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Audit.Abstractions;
using Common.Audit.Enums;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Domain.Enums;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.CreateAuction;

public class CreateAuctionCommandHandler : ICommandHandler<CreateAuctionCommand, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<CreateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IFileConfirmationService _fileConfirmationService;

    public CreateAuctionCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<CreateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher,
        IFileConfirmationService fileConfirmationService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
        _fileConfirmationService = fileConfirmationService;
    }

    public async Task<Result<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating auction for seller {Seller} at {Timestamp}", request.Seller, _dateTime.UtcNow);

        try
        {
            // Create the auction entity
            var auction = CreateAuctionEntity(request);

            // Persist the auction
            var createdAuction = await _repository.CreateAsync(auction, cancellationToken);

            // Confirm pre-uploaded files (Status 1 → 2)
            if (request.FileIds != null && request.FileIds.Count > 0)
            {
                _logger.LogInformation("Confirming {FileCount} temp files for auction {AuctionId}", 
                    request.FileIds.Count, createdAuction.Id);

                await _fileConfirmationService.ConfirmFilesAsync(
                    request.FileIds, 
                    "Auction", 
                    createdAuction.Id.ToString(), 
                    cancellationToken);

                _logger.LogInformation("Confirmed {FileCount} files for auction {AuctionId}", 
                    request.FileIds.Count, createdAuction.Id);
            }

            // Publish the event
            var auctionCreatedEvent = _mapper.Map<AuctionCreatedEvent>(createdAuction);
            await _eventPublisher.PublishAsync(auctionCreatedEvent, cancellationToken);

            // Publish audit event
            await _auditPublisher.PublishAsync(
                createdAuction.Id,
                createdAuction,
                AuditAction.Created,
                cancellationToken: cancellationToken);

            // Save changes (including outbox)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created auction {AuctionId} for seller {Seller}", createdAuction.Id, request.Seller);

            var dto = _mapper.Map<AuctionDto>(createdAuction);
            return Result<AuctionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create auction for seller {Seller}: {Error}", request.Seller, ex.Message);
            return Result.Failure<AuctionDto>(Error.Create("Auction.CreateFailed", $"Failed to create auction: {ex.Message}"));
        }
    }

    private Auction CreateAuctionEntity(CreateAuctionCommand request)
    {
        return new Auction
        {
            Id = Guid.NewGuid(),
            Seller = request.Seller,
            ReversePrice = request.ReservePrice,
            AuctionEnd = request.AuctionEnd,
            Status = Status.Live,
            Item = new Item
            {
                Title = request.Title,
                Description = request.Description,
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Color = request.Color,
                Mileage = request.Mileage
            }
        };
    }
}
