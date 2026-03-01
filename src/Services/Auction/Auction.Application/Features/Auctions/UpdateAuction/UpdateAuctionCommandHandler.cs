using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Auctions.Application.Commands.UpdateAuction;

public class UpdateAuctionCommandHandler : ICommandHandler<UpdateAuctionCommand, bool>
{
    private readonly IAuctionWriteRepository _repository;
    private readonly ILogger<UpdateAuctionCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISanitizationService _sanitizationService;

    public UpdateAuctionCommandHandler(
        IAuctionWriteRepository repository,
        ILogger<UpdateAuctionCommandHandler> logger,
        IUnitOfWork unitOfWork,
        ISanitizationService sanitizationService)
    {
        _repository = repository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _sanitizationService = sanitizationService;
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
            return Result.Failure<bool>(Error.Create("Auction.Forbidden", "You are not authorized to update this auction"));
        }

        var modifiedFields = new List<string>();

        if (request.Title != null && request.Title != auction.Item.Title)
        {
            auction.Item.UpdateTitle(_sanitizationService.SanitizeText(request.Title));
            modifiedFields.Add(nameof(auction.Item.Title));
        }

        if (request.Description != null && request.Description != auction.Item.Description)
        {
            auction.Item.UpdateDescription(_sanitizationService.SanitizeHtml(request.Description));
            modifiedFields.Add(nameof(auction.Item.Description));
        }

        if (request.Condition != null && request.Condition != auction.Item.Condition)
        {
            auction.Item.UpdateCondition(request.Condition);
            modifiedFields.Add(nameof(auction.Item.Condition));
        }

        if (request.YearManufactured != null && request.YearManufactured != auction.Item.YearManufactured)
        {
            auction.Item.UpdateYearManufactured(request.YearManufactured);
            modifiedFields.Add(nameof(auction.Item.YearManufactured));
        }

        if (request.Attributes != null)
        {
            foreach (var attr in request.Attributes)
            {
                auction.Item.SetAttribute(attr.Key, attr.Value);
            }
            modifiedFields.Add(nameof(auction.Item.Attributes));
        }

        auction.RaiseUpdatedEvent(modifiedFields);

        await _repository.UpdateAsync(auction, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated auction {AuctionId} with fields: {ModifiedFields}",
            request.Id, string.Join(", ", modifiedFields));
        return Result.Success(true);
    }
}
