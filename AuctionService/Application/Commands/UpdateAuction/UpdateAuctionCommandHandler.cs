using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using Common.Audit.Abstractions;
using Common.Core.Helpers;
using Common.Core.Interfaces;
using Common.CQRS.Abstractions;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;

namespace AuctionService.Application.Commands.UpdateAuction;

/// <summary>
/// Handler for UpdateAuctionCommand - demonstrates CQRS pattern with partial updates
/// </summary>
public class UpdateAuctionCommandHandler : ICommandHandler<UpdateAuctionCommand, bool>
{
    private readonly IAuctionRepository _repository;
    private readonly IAppLogger<UpdateAuctionCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public UpdateAuctionCommandHandler(
        IAuctionRepository repository,
        IAppLogger<UpdateAuctionCommandHandler> logger,
        IDateTimeProvider dateTime,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _logger = logger;
        _dateTime = dateTime;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<bool>> Handle(UpdateAuctionCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating auction {AuctionId} at {Timestamp}", request.Id, _dateTime.UtcNow);

        try
        {
            var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (auction == null)
            {
                _logger.LogWarning("Auction {AuctionId} not found for update", request.Id);
                return Result.Failure<bool>(Error.Create("Auction.NotFound", $"Auction with ID {request.Id} was not found"));
            }

            // Capture old values for audit (using a copy of the auction)
            var oldAuction = new Auction
            {
                Id = auction.Id,
                Seller = auction.Seller,
                ReversePrice = auction.ReversePrice,
                AuctionEnd = auction.AuctionEnd,
                Status = auction.Status,
                Item = new Item
                {
                    Title = auction.Item.Title,
                    Description = auction.Item.Description,
                    Make = auction.Item.Make,
                    Model = auction.Item.Model,
                    Color = auction.Item.Color,
                    Mileage = auction.Item.Mileage,
                    Year = auction.Item.Year
                }
            };

            // Apply partial updates
            auction.Item.Title = request.Title ?? auction.Item.Title;
            auction.Item.Description = request.Description ?? auction.Item.Description;
            auction.Item.Make = request.Make ?? auction.Item.Make;
            auction.Item.Model = request.Model ?? auction.Item.Model;
            auction.Item.Color = request.Color ?? auction.Item.Color;
            auction.Item.Mileage = request.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = request.Year ?? auction.Item.Year;

            await _repository.UpdateAsync(auction, cancellationToken);

            // Publish event
            await _eventPublisher.PublishAsync(new AuctionUpdatedEvent
            {
                Id = request.Id,
                Seller = auction.Seller,
                Title = request.Title,
                Description = request.Description,
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                Color = request.Color,
                Mileage = request.Mileage
            }, cancellationToken);

            // Publish audit event with old and new values
            await _auditPublisher.PublishAsync(
                auction.Id,
                auction,
                Common.Audit.Enums.AuditAction.Updated,
                oldAuction,
                cancellationToken: cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated auction {AuctionId}", request.Id);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to update auction {AuctionId}: {Error}", request.Id, ex.Message);
            return Result.Failure<bool>(Error.Create("Auction.UpdateFailed", $"Failed to update auction: {ex.Message}"));
        }
    }
}
