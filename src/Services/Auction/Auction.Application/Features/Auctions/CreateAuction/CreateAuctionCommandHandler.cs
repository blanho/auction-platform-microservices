using Auctions.Application.DTOs;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Auctions.Application.Errors;
using Auctions.Domain.Enums;
using Microsoft.Extensions.Logging;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Auctions.CreateAuction;

public class CreateAuctionCommandHandler : ICommandHandler<CreateAuctionCommand, AuctionDto>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISanitizationService _sanitizationService;
    private readonly IAuditPublisher _auditPublisher;
    private readonly ICatalogGrpcClient _catalogClient;

    public CreateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        IMapper mapper,
        ILogger<CreateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ISanitizationService sanitizationService,
        IAuditPublisher auditPublisher,
        ICatalogGrpcClient catalogClient)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _sanitizationService = sanitizationService;
        _auditPublisher = auditPublisher;
        _catalogClient = catalogClient;
    }

    public async Task<Result<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating auction for seller {Seller} at {Timestamp}", request.SellerUsername, _dateTime.UtcNow);

        string? brandName = null;
        if (request.BrandId.HasValue)
        {
            var brandInfo = await _catalogClient.GetBrandAsync(request.BrandId.Value, cancellationToken);
            if (brandInfo == null)
                return Result.Failure<AuctionDto>(AuctionErrors.Brand.NotFoundById(request.BrandId.Value));
            brandName = brandInfo.Value.Name;
        }

        string? categoryName = null;
        if (request.CategoryId.HasValue)
        {
            var categoryInfo = await _catalogClient.GetCategoryAsync(request.CategoryId.Value, cancellationToken);
            if (categoryInfo == null)
                return Result.Failure<AuctionDto>(AuctionErrors.Category.NotFoundById(request.CategoryId.Value));
            categoryName = categoryInfo.Value.Name;
        }

        var auction = CreateAuctionEntity(request, brandName, categoryName);

        var createdAuction = await _repository.CreateAsync(auction, cancellationToken);

        await _auditPublisher.PublishAsync(
            createdAuction.Id,
            AuctionAuditData.FromAuction(createdAuction),
            AuditAction.Created,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created auction {AuctionId} for seller {Seller}", createdAuction.Id, request.SellerUsername);

        var dto = _mapper.Map<AuctionDto>(createdAuction);
        return Result<AuctionDto>.Success(dto);
    }

    private Auction CreateAuctionEntity(CreateAuctionCommand request, string? brandName, string? categoryName)
    {
        var item = Item.Create(new CreateItemParams
        {
            Title = _sanitizationService.SanitizeText(request.Title),
            Description = _sanitizationService.SanitizeHtml(request.Description),
            Condition = request.Condition,
            YearManufactured = request.YearManufactured,
            CategoryId = request.CategoryId,
            CategoryName = categoryName,
            BrandId = request.BrandId,
            BrandName = brandName
        });

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
                auction.Item.AddFile(MediaFile.Create(
                    file.FileId,
                    file.FileType,
                    file.DisplayOrder,
                    file.IsPrimary));
            }
        }

        return auction;
    }
}

