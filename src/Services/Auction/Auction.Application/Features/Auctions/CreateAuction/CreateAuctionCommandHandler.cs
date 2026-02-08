using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using Auctions.Domain.Enums;
using Microsoft.Extensions.Logging;
// using BuildingBlocks.Infrastructure.Caching; // Use BuildingBlocks.Application.Abstractions instead
// using BuildingBlocks.Infrastructure.Repository; // Use BuildingBlocks.Application.Abstractions instead

namespace Auctions.Application.Commands.CreateAuction;

public class CreateAuctionCommandHandler : ICommandHandler<CreateAuctionCommand, AuctionDto>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISanitizationService _sanitizationService;

    public CreateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        IMapper mapper,
        ILogger<CreateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ISanitizationService sanitizationService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _sanitizationService = sanitizationService;
    }

    public async Task<Result<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating auction for seller {Seller} at {Timestamp}", request.SellerUsername, _dateTime.UtcNow);

        var auction = CreateAuctionEntity(request);
        
        auction.RaiseCreatedEvent();

        var createdAuction = await _repository.CreateAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created auction {AuctionId} for seller {Seller}", createdAuction.Id, request.SellerUsername);

        var dto = _mapper.Map<AuctionDto>(createdAuction);
        return Result<AuctionDto>.Success(dto);
    }

    private Auction CreateAuctionEntity(CreateAuctionCommand request)
    {
        var item = Item.Create(
            title: _sanitizationService.SanitizeText(request.Title),
            description: _sanitizationService.SanitizeHtml(request.Description),
            condition: request.Condition,
            yearManufactured: request.YearManufactured,
            categoryId: request.CategoryId,
            brandId: request.BrandId);

        if (request.Attributes != null)
        {
            foreach (var attr in request.Attributes)
            {
                item.SetAttribute(attr.Key, attr.Value);
            }
        }

        var auction = Auction.Create(new CreateAuctionParams(
            SellerId: request.SellerId,
            SellerUsername: request.SellerUsername,
            Item: item,
            ReservePrice: request.ReservePrice,
            AuctionEnd: request.AuctionEnd,
            Currency: request.Currency,
            BuyNowPrice: request.BuyNowPrice,
            IsFeatured: request.IsFeatured));

        if (request.Files != null && request.Files.Count > 0)
        {
            foreach (var file in request.Files)
            {
                auction.Item.AddFile(new MediaFile
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

