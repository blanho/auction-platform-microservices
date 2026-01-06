using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Audit.Abstractions;
using Common.Caching.Abstractions;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.UpdateAuction;

public class UpdateAuctionCommandHandler : ICommandHandler<UpdateAuctionCommand, bool>
{
    private readonly IAuctionRepository _repository;
    private readonly IAppLogger<UpdateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;
    private readonly ICacheService _cache;
    
    private const string CacheKeyPrefix = "auction:";

    public UpdateAuctionCommandHandler(
        IAuctionRepository repository,
        IAppLogger<UpdateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher,
        ICacheService cache)
    {
        _repository = repository;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating auction {AuctionId} by user {UserId} at {Timestamp}", 
            request.Id, request.RequestingUserId, _dateTime.UtcNow);

        try
        {
            var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (auction == null)
            {
                _logger.LogWarning("Auction {AuctionId} not found for update", request.Id);
                return Result.Failure<bool>(Error.Create("Auction.NotFound", $"Auction with ID {request.Id} was not found"));
            }
            if (!request.IsAdmin && auction.SellerId != request.RequestingUserId)
            {
                _logger.LogWarning(
                    "Unauthorized update attempt on auction {AuctionId} by user {UserId}. Owner is {SellerId}",
                    request.Id, request.RequestingUserId, auction.SellerId);
                return Result.Failure<bool>(Error.Create("Auction.Unauthorized", "You are not authorized to update this auction"));
            }

            var oldAuction = new Auction
            {
                Id = auction.Id,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                ReservePrice = auction.ReservePrice,
                AuctionEnd = auction.AuctionEnd,
                Status = auction.Status,
                Item = new Item
                {
                    Title = auction.Item.Title,
                    Description = auction.Item.Description,
                    Condition = auction.Item.Condition,
                    YearManufactured = auction.Item.YearManufactured
                }
            };
            var modifiedFields = new List<string>();
            
            if (request.Title != null && request.Title != auction.Item.Title)
            {
                auction.Item.Title = request.Title;
                modifiedFields.Add(nameof(auction.Item.Title));
            }
            
            if (request.Description != null && request.Description != auction.Item.Description)
            {
                auction.Item.Description = request.Description;
                modifiedFields.Add(nameof(auction.Item.Description));
            }
            
            if (request.Condition != null && request.Condition != auction.Item.Condition)
            {
                auction.Item.Condition = request.Condition;
                modifiedFields.Add(nameof(auction.Item.Condition));
            }
            
            if (request.YearManufactured != null && request.YearManufactured != auction.Item.YearManufactured)
            {
                auction.Item.YearManufactured = request.YearManufactured;
                modifiedFields.Add(nameof(auction.Item.YearManufactured));
            }
            
            if (request.Attributes != null)
            {
                foreach (var attr in request.Attributes)
                {
                    auction.Item.Attributes[attr.Key] = attr.Value;
                }
                modifiedFields.Add(nameof(auction.Item.Attributes));
            }

            auction.RaiseUpdatedEvent(modifiedFields);
            
            await _repository.UpdateAsync(auction, cancellationToken);

            await _auditPublisher.PublishAsync(
                auction.Id,
                auction,
                Common.Audit.Enums.AuditAction.Updated,
                oldAuction,
                cancellationToken: cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _cache.RemoveAsync($"{CacheKeyPrefix}{request.Id}", cancellationToken);

            _logger.LogInformation("Updated auction {AuctionId} with fields: {ModifiedFields}", 
                request.Id, string.Join(", ", modifiedFields));
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update auction {AuctionId}: {Error}", request.Id, ex.Message);
            return Result.Failure<bool>(Error.Create("Auction.UpdateFailed", $"Failed to update auction: {ex.Message}"));
        }
    }
}
