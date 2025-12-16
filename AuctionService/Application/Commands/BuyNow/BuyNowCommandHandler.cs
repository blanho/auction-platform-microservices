using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
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

namespace AuctionService.Application.Commands.BuyNow;

public class BuyNowCommandHandler : ICommandHandler<BuyNowCommand, BuyNowResultDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<BuyNowCommandHandler> _logger;
    private readonly IDateTimeProvider _dateTime;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditPublisher _auditPublisher;

    public BuyNowCommandHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<BuyNowCommandHandler> logger,
        IDateTimeProvider dateTime,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result<BuyNowResultDto>> Handle(BuyNowCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Buy Now for auction {AuctionId} by buyer {Buyer}", 
            request.AuctionId, request.BuyerUsername);

        try
        {
            var auction = await _repository.GetByIdAsync(request.AuctionId, cancellationToken);
            
            if (auction == null)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("Auction.NotFound", "Auction not found"));
            }

            if (!auction.IsBuyNowAvailable)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("BuyNow.NotAvailable", "Buy Now is not available for this auction"));
            }

            if (auction.SellerUsername == request.BuyerUsername)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("BuyNow.OwnAuction", "You cannot buy your own auction"));
            }

            if (auction.Status != Status.Live)
            {
                return Result.Failure<BuyNowResultDto>(
                    Error.Create("BuyNow.AuctionNotLive", "This auction is no longer active"));
            }

            auction.WinnerId = request.BuyerId;
            auction.WinnerUsername = request.BuyerUsername;
            auction.SoldAmount = auction.BuyNowPrice;
            auction.Status = Status.Finished;

            await _repository.UpdateAsync(auction, cancellationToken);

            var buyNowEvent = new BuyNowExecutedEvent
            {
                AuctionId = auction.Id,
                Buyer = request.BuyerUsername,
                Seller = auction.SellerUsername,
                BuyNowPrice = (int)auction.BuyNowPrice!.Value,
                ItemTitle = auction.Item.Title,
                ExecutedAt = DateTime.UtcNow
            };
            await _eventPublisher.PublishAsync(buyNowEvent, cancellationToken);

            var auctionFinishedEvent = new AuctionFinishedEvent
            {
                ItemSold = true,
                AuctionId = auction.Id,
                WinnerId = request.BuyerId,
                WinnerUsername = request.BuyerUsername,
                SellerId = auction.SellerId,
                SellerUsername = auction.SellerUsername,
                SoldAmount = auction.BuyNowPrice
            };
            await _eventPublisher.PublishAsync(auctionFinishedEvent, cancellationToken);

            await _auditPublisher.PublishAsync(
                auction.Id,
                auction,
                AuditAction.Updated,
                cancellationToken: cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Buy Now completed for auction {AuctionId}. Buyer: {Buyer}, Price: {Price}", 
                auction.Id, request.BuyerUsername, auction.BuyNowPrice);

            return Result<BuyNowResultDto>.Success(new BuyNowResultDto
            {
                AuctionId = auction.Id,
                Buyer = request.BuyerUsername,
                Seller = auction.SellerUsername,
                BuyNowPrice = (int)auction.BuyNowPrice!.Value,
                ItemTitle = auction.Item.Title,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Buy Now failed for auction {AuctionId}: {Error}", request.AuctionId, ex.Message);
            return Result.Failure<BuyNowResultDto>(
                Error.Create("BuyNow.Failed", $"Failed to process Buy Now: {ex.Message}"));
        }
    }
}
