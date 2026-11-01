using Auctions.Application.DTOs;
using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Microsoft.Extensions.Logging;
using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Auctions.UpdateAuction;

public class UpdateAuctionCommandHandler : ICommandHandler<UpdateAuctionCommand, bool>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<UpdateAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISanitizationService _sanitizationService;
    private readonly IAuditPublisher _auditPublisher;
    private readonly ICatalogGrpcClient _catalogClient;

    public UpdateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<UpdateAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISanitizationService sanitizationService,
        IAuditPublisher auditPublisher,
        ICatalogGrpcClient catalogClient)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _sanitizationService = sanitizationService;
        _auditPublisher = auditPublisher;
        _catalogClient = catalogClient;
    }

    public async Task<Result<bool>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating auction {AuctionId}", request.Id);

        var auction = await _repository.GetByIdForUpdateAsync(request.Id, cancellationToken);

        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for update", request.Id);
            return Result.Failure<bool>(AuctionErrors.Auction.NotFoundById(request.Id));
        }

        if (auction.SellerId != request.UserId)
        {
            _logger.LogWarning("User {UserId} attempted to update auction {AuctionId} owned by {OwnerId}",
                request.UserId, request.Id, auction.SellerId);
            return Result.Failure<bool>(AuctionErrors.Auction.Forbidden);
        }

        var oldAuctionData = AuctionAuditData.FromAuction(auction);

        var modifiedFields = new List<string>();

        UpdateItemDetails(auction.Item, request, modifiedFields);
        UpdatePrices(auction, request, modifiedFields);

        var catalogError = await UpdateCatalogAsync(auction.Item, request, modifiedFields, cancellationToken);
        if (catalogError is not null)
        {
            return Result.Failure<bool>(catalogError);
        }

        UpdateListingDetails(auction, request, modifiedFields);

        auction.RaiseUpdatedEvent(modifiedFields);

        await _repository.UpdateAsync(auction, cancellationToken);

        await _auditPublisher.PublishAsync(
            auction.Id,
            AuctionAuditData.FromAuction(auction),
            AuditAction.Updated,
            oldAuctionData,
            AuctionAuditMetadata.ForModifiedFields(modifiedFields),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated auction {AuctionId} with fields: {ModifiedFields}",
            request.Id, string.Join(", ", modifiedFields));
        return Result.Success(true);
    }

    private void UpdateItemDetails(Item item, UpdateAuctionCommand request, List<string> modifiedFields)
    {
        if (request.Title != null && request.Title != item.Title)
        {
            item.UpdateTitle(_sanitizationService.SanitizeText(request.Title));
            modifiedFields.Add(nameof(item.Title));
        }

        if (request.Description != null && request.Description != item.Description)
        {
            item.UpdateDescription(_sanitizationService.SanitizeHtml(request.Description));
            modifiedFields.Add(nameof(item.Description));
        }

        if (request.Condition != null && request.Condition != item.Condition)
        {
            item.UpdateCondition(request.Condition);
            modifiedFields.Add(nameof(item.Condition));
        }

        if (request.YearManufactured != null && request.YearManufactured != item.YearManufactured)
        {
            item.UpdateYearManufactured(request.YearManufactured);
            modifiedFields.Add(nameof(item.YearManufactured));
        }

        if (request.Attributes != null)
        {
            foreach (var attribute in request.Attributes)
            {
                item.SetAttribute(attribute.Key, attribute.Value);
            }
            modifiedFields.Add(nameof(item.Attributes));
        }
    }

    private static void UpdatePrices(Auction auction, UpdateAuctionCommand request, List<string> modifiedFields)
    {
        if (request.ReservePrice.HasValue && request.ReservePrice.Value != auction.ReservePrice)
        {
            auction.UpdateReservePrice(request.ReservePrice.Value);
            modifiedFields.Add(nameof(auction.ReservePrice));
        }

        if (request.BuyNowPrice != null && request.BuyNowPrice != auction.BuyNowPrice)
        {
            auction.UpdateBuyNowPrice(request.BuyNowPrice);
            modifiedFields.Add(nameof(auction.BuyNowPrice));
        }
    }

    private async Task<Error?> UpdateCatalogAsync(
        Item item,
        UpdateAuctionCommand request,
        List<string> modifiedFields,
        CancellationToken cancellationToken)
    {
        if (request.CategoryId.HasValue && request.CategoryId != item.CategoryId)
        {
            var categoryInfo = await _catalogClient.GetCategoryAsync(request.CategoryId.Value, cancellationToken);
            if (categoryInfo is null)
            {
                return AuctionErrors.Category.NotFoundById(request.CategoryId.Value);
            }

            item.UpdateCategory(request.CategoryId, categoryInfo.Value.Name);
            modifiedFields.Add(nameof(item.CategoryId));
        }

        if (request.BrandId.HasValue && request.BrandId != item.BrandId)
        {
            var brandInfo = await _catalogClient.GetBrandAsync(request.BrandId.Value, cancellationToken);
            if (brandInfo is null)
            {
                return AuctionErrors.Brand.NotFoundById(request.BrandId.Value);
            }

            item.UpdateBrand(request.BrandId, brandInfo.Value.Name);
            modifiedFields.Add(nameof(item.BrandId));
        }

        return null;
    }

    private static void UpdateListingDetails(Auction auction, UpdateAuctionCommand request, List<string> modifiedFields)
    {
        if (request.IsFeatured.HasValue && request.IsFeatured.Value != auction.IsFeatured)
        {
            auction.SetFeatured(request.IsFeatured.Value);
            modifiedFields.Add(nameof(auction.IsFeatured));
        }

        if (request.AuctionEnd.HasValue && request.AuctionEnd.Value != auction.AuctionEnd)
        {
            var extension = request.AuctionEnd.Value - auction.AuctionEnd;
            auction.ExtendAuctionEnd(extension);
            modifiedFields.Add(nameof(auction.AuctionEnd));
        }
    }
}
