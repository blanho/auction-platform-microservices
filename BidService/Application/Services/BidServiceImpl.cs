using AutoMapper;
using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using BidService.Domain.Entities;
using BidService.Domain.ValueObjects;
using Common.Core.Interfaces;
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

        public BidServiceImpl(
            IBidRepository repository,
            IMapper mapper,
            IAppLogger<BidServiceImpl> logger,
            IDateTimeProvider dateTime,
            IEventPublisher eventPublisher,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
        }

        public async Task<BidDto> PlaceBidAsync(PlaceBidDto dto, string bidder, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Placing bid for auction {AuctionId} by bidder {Bidder}", dto.AuctionId, bidder);

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
            else if (dto.Amount == highestBid.Amount)
            {
                bid.Status = BidStatus.TooLow;
            }
            else
            {
                bid.Status = BidStatus.TooLow;
            }

            var createdBid = await _repository.CreateAsync(bid, cancellationToken);

            var bidPlacedEvent = _mapper.Map<BidPlacedEvent>(createdBid);
            await _eventPublisher.PublishAsync(bidPlacedEvent, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
