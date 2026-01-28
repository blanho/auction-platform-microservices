using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Domain.Enums;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.CreateAuction;

public class CreateAuctionCommandHandler : ICommandHandler<CreateAuctionCommand, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileConfirmationService _fileConfirmationService;
    private readonly ISanitizationService _sanitizationService;

    public CreateAuctionCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        ILogger<CreateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        IFileConfirmationService fileConfirmationService,
        ISanitizationService sanitizationService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _fileConfirmationService = fileConfirmationService;
        _sanitizationService = sanitizationService;
    }

    public async Task<Result<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating auction for seller {Seller} at {Timestamp}", request.SellerUsername, _dateTime.UtcNow);

        var auction = CreateAuctionEntity(request);
        
        auction.RaiseCreatedEvent();

        var createdAuction = await _repository.CreateAsync(auction, cancellationToken);

        if (request.Files != null && request.Files.Count > 0)
        {
            var fileIds = request.Files.Select(f => f.FileId).ToList();
            
            _logger.LogInformation("Confirming {FileCount} files for auction {AuctionId}", 
                fileIds.Count, createdAuction.Id);

            await _fileConfirmationService.ConfirmFilesAsync(
                fileIds, 
                "Auction", 
                createdAuction.Id.ToString(), 
                cancellationToken);

            _logger.LogInformation("Confirmed {FileCount} files for auction {AuctionId}", 
                fileIds.Count, createdAuction.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created auction {AuctionId} for seller {Seller}", createdAuction.Id, request.SellerUsername);

        var dto = _mapper.Map<AuctionDto>(createdAuction);
        return Result<AuctionDto>.Success(dto);
    }

    private Auction CreateAuctionEntity(CreateAuctionCommand request)
    {
        var item = new Item
        {
            Title = _sanitizationService.SanitizeText(request.Title),
            Description = _sanitizationService.SanitizeHtml(request.Description),
            Condition = request.Condition,
            YearManufactured = request.YearManufactured,
            Attributes = request.Attributes ?? new Dictionary<string, string>(),
            CategoryId = request.CategoryId,
            Files = new List<MediaFile>()
        };

        var auction = Auction.Create(
            sellerId: request.SellerId,
            sellerUsername: request.SellerUsername,
            item: item,
            reservePrice: request.ReservePrice,
            auctionEnd: request.AuctionEnd,
            currency: request.Currency,
            buyNowPrice: request.BuyNowPrice,
            isFeatured: request.IsFeatured);

        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                auction.Item.Files.Add(new MediaFile
                {
                    FileId = file.FileId,
                    FileType = file.FileType,
                    DisplayOrder = file.DisplayOrder,
                    IsPrimary = file.IsPrimary
                });
            }
        }

        return auction;
    }
}

