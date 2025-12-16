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
        _logger.LogInformation("Creating auction for seller {Seller} at {Timestamp}", request.SellerUsername, _dateTime.UtcNow);

        try
        {
            var auction = CreateAuctionEntity(request);

            var createdAuction = await _repository.CreateAsync(auction, cancellationToken);

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

            var auctionCreatedEvent = _mapper.Map<AuctionCreatedEvent>(createdAuction);
            await _eventPublisher.PublishAsync(auctionCreatedEvent, cancellationToken);

            await _auditPublisher.PublishAsync(
                createdAuction.Id,
                createdAuction,
                AuditAction.Created,
                cancellationToken: cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created auction {AuctionId} for seller {Seller}", createdAuction.Id, request.SellerUsername);

            var dto = _mapper.Map<AuctionDto>(createdAuction);
            return Result<AuctionDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create auction for seller {Seller}: {Error}", request.SellerUsername, ex.Message);
            return Result.Failure<AuctionDto>(Error.Create("Auction.CreateFailed", $"Failed to create auction: {ex.Message}"));
        }
    }

    private Auction CreateAuctionEntity(CreateAuctionCommand request)
    {
        var auctionId = Guid.NewGuid();
        var auction = new Auction
        {
            Id = auctionId,
            SellerId = request.SellerId,
            SellerUsername = request.SellerUsername,
            ReservePrice = request.ReservePrice,
            BuyNowPrice = request.BuyNowPrice,
            Currency = request.Currency,
            AuctionEnd = request.AuctionEnd,
            Status = Status.Live,
            IsFeatured = request.IsFeatured,
            Item = new Item
            {
                Title = request.Title,
                Description = request.Description,
                Condition = request.Condition,
                YearManufactured = request.YearManufactured,
                Attributes = request.Attributes ?? new Dictionary<string, string>(),
                CategoryId = request.CategoryId,
                Files = new List<ItemFileInfo>()
            }
        };

        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                auction.Item.Files.Add(new ItemFileInfo
                {
                    StorageFileId = Guid.NewGuid(),
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Size = file.Size,
                    Url = file.Url,
                    FileType = "image",
                    DisplayOrder = file.DisplayOrder,
                    IsPrimary = file.IsPrimary,
                    UploadedAt = DateTimeOffset.UtcNow
                });
            }
        }

        return auction;
    }
}
