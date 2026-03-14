using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Audit;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Auctions.Domain.Enums;
using Microsoft.Extensions.Logging;

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

    public CreateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        IMapper mapper,
        ILogger<CreateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ISanitizationService sanitizationService,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _sanitizationService = sanitizationService;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<AuctionDto>> Handle(CreateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating auction for seller {Seller} at {Timestamp}", request.SellerUsername, _dateTime.UtcNow);

        var auction = CreateAuctionEntity(request);

        var createdAuction = await _repository.CreateAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            createdAuction.Id,
            AuctionAuditData.FromAuction(createdAuction),
            AuditAction.Created,
            cancellationToken: cancellationToken);

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

