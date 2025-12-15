using AutoMapper;
using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using BidService.Domain.Entities;
using BidService.Domain.ValueObjects;
using Common.Core.Interfaces;
using Common.Locking.Abstractions;
using Common.Messaging.Abstractions;
using Common.Messaging.Events;
using Common.Repository.Interfaces;

namespace BidService.Application.Services
{
    public class BidServiceImpl : IBidService
    {
        private readonly IBidRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAppLogger<BidServiceImpl> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuctionValidationService _auctionValidation;
        private readonly IDistributedLock _distributedLock;
        private const int AntiSnipeThresholdMinutes = 2;
        private const int AntiSnipeExtensionMinutes = 2;

        public BidServiceImpl(
            IBidRepository repository,
            IMapper mapper,
            IAppLogger<BidServiceImpl> logger,
            IDateTimeProvider dateTime,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            IAuctionValidationService auctionValidation,
            IDistributedLock distributedLock)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
            _auctionValidation = auctionValidation;
            _distributedLock = distributedLock;
        }

        public async Task<BidDto> PlaceBidAsync(PlaceBidDto dto, string bidder, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Placing bid for auction {AuctionId} by bidder {Bidder}", dto.AuctionId, bidder);

            var validationResult = await _auctionValidation.ValidateAuctionForBidAsync(
                dto.AuctionId, bidder, dto.Amount, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Auction validation failed for {AuctionId}: {ErrorCode} - {ErrorMessage}",
                    dto.AuctionId, validationResult.ErrorCode, validationResult.ErrorMessage);

                return new BidDto
                {
                    AuctionId = dto.AuctionId,
                    Bidder = bidder,
                    Amount = dto.Amount,
                    BidTime = _dateTime.UtcNow,
                    Status = BidStatus.Rejected.ToString(),
                    ErrorMessage = validationResult.ErrorMessage
                };
            }

            var lockKey = $"auction-bid:{dto.AuctionId}";
            await using var lockHandle = await _distributedLock.TryAcquireAsync(
                lockKey,
                TimeSpan.FromSeconds(10),
                cancellationToken);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for auction {AuctionId}", dto.AuctionId);
                return new BidDto
                {
                    AuctionId = dto.AuctionId,
                    Bidder = bidder,
                    Amount = dto.Amount,
                    BidTime = _dateTime.UtcNow,
                    Status = BidStatus.Rejected.ToString(),
                    ErrorMessage = "Another bid is being processed. Please try again."
                };
            }

            var highestBid = await _repository.GetHighestBidForAuctionAsync(dto.AuctionId, cancellationToken);
            var currentHighBid = highestBid?.Amount ?? 0;

            if (!BidIncrement.IsValidBidAmount(dto.Amount, currentHighBid))
            {
                var minimumNextBid = BidIncrement.GetMinimumNextBid(currentHighBid);
                _logger.LogWarning(
                    "Bid amount {Amount} does not meet minimum increment. Current high: {CurrentHigh}, Minimum next: {MinimumNext}",
                    dto.Amount, currentHighBid, minimumNextBid);
                
                return new BidDto
                {
                    AuctionId = dto.AuctionId,
                    Bidder = bidder,
                    Amount = dto.Amount,
                    BidTime = _dateTime.UtcNow,
                    Status = BidStatus.TooLow.ToString(),
                    ErrorMessage = BidIncrement.GetIncrementErrorMessage(dto.Amount, currentHighBid)
                };
            }

            var bid = _mapper.Map<Bid>(dto);
            bid.Bidder = bidder;
            bid.BidTime = _dateTime.UtcNow;

            if (highestBid == null)
            {
                bid.Status = BidStatus.Accepted;
            }
            else if (dto.Amount > highestBid.Amount)
            {
                bid.Status = BidStatus.Accepted;
            }
            else
            {
                bid.Status = BidStatus.TooLow;
            }

            var createdBid = await _repository.CreateAsync(bid, cancellationToken);

            var bidPlacedEvent = _mapper.Map<BidPlacedEvent>(createdBid);
            await _eventPublisher.PublishAsync(bidPlacedEvent, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (bid.Status == BidStatus.Accepted && validationResult.AuctionEnd.HasValue)
            {
                var timeRemaining = validationResult.AuctionEnd.Value - DateTimeOffset.UtcNow;
                if (timeRemaining.TotalMinutes < AntiSnipeThresholdMinutes)
                {
                    _logger.LogInformation(
                        "Anti-snipe protection triggered for auction {AuctionId}. Extending by {Minutes} minutes",
                        dto.AuctionId, AntiSnipeExtensionMinutes);

                    await _auctionValidation.ExtendAuctionAsync(
                        dto.AuctionId,
                        AntiSnipeExtensionMinutes,
                        "Anti-snipe protection",
                        cancellationToken);
                }
            }

            _logger.LogInformation("Bid {BidId} placed for auction {AuctionId} with status {Status}", 
                createdBid.Id, dto.AuctionId, bid.Status);

            return _mapper.Map<BidDto>(createdBid);
        }

        public async Task<List<BidDto>> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting bids for auction {AuctionId}", auctionId);
            var bids = await _repository.GetBidsByAuctionIdAsync(auctionId, cancellationToken);
            return _mapper.Map<List<BidDto>>(bids);
        }

        public async Task<List<BidDto>> GetBidsForBidderAsync(string bidder, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting bids for bidder {Bidder}", bidder);
            var bids = await _repository.GetBidsByBidderAsync(bidder, cancellationToken);
            return _mapper.Map<List<BidDto>>(bids);
        }
    }
}
