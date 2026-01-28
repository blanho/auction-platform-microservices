using Auctions.Application.Errors;
using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Commands.UpdateAuction;

public class UpdateAuctionCommandHandler : ICommandHandler<UpdateAuctionCommand, bool>
{
    private readonly IAuctionRepository _repository;
    private readonly ILogger<UpdateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ISanitizationService _sanitizationService;
    
    private const string CacheKeyPrefix = "auction:";

    public UpdateAuctionCommandHandler(
        IAuctionRepository repository,
        ILogger<UpdateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ISanitizationService sanitizationService)
    {
        _repository = repository;
        _logger = logger;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _sanitizationService = sanitizationService;
    }

    public async Task<Result<bool>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating auction {AuctionId} at {Timestamp}", 
            request.Id, _dateTime.UtcNow);

        var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (auction == null)
        {
            _logger.LogWarning("Auction {AuctionId} not found for update", request.Id);
            return Result.Failure<bool>(AuctionErrors.Auction.NotFoundById(request.Id));
        }

        var modifiedFields = new List<string>();
        
        if (request.Title != null && request.Title != auction.Item.Title)
        {
            auction.Item.Title = _sanitizationService.SanitizeText(request.Title);
            modifiedFields.Add(nameof(auction.Item.Title));
        }
        
        if (request.Description != null && request.Description != auction.Item.Description)
        {
            auction.Item.Description = _sanitizationService.SanitizeHtml(request.Description);
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"{CacheKeyPrefix}{request.Id}", cancellationToken);

        _logger.LogInformation("Updated auction {AuctionId} with fields: {ModifiedFields}", 
            request.Id, string.Join(", ", modifiedFields));
        return Result.Success(true);
    }
}
