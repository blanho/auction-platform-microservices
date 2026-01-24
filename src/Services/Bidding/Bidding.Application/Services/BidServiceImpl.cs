using UnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

namespace Bidding.Application.Services
{
    public class BidServiceImpl : IBidService
    {
        private readonly IBidRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<BidServiceImpl> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEventPublisher _eventPublisher;
        private readonly UnitOfWork _unitOfWork;
        private readonly IDistributedLock _distributedLock;

        public BidServiceImpl(
            IBidRepository repository,
            IMapper mapper,
            ILogger<BidServiceImpl> logger,
            IDateTimeProvider dateTime,
            IEventPublisher eventPublisher,
            UnitOfWork unitOfWork,
            IDistributedLock distributedLock)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
            _distributedLock = distributedLock;
        }

        public async Task<BidDto> PlaceBidAsync(PlaceBidDto dto, Guid bidderId, string bidderUsername, CancellationToken cancellationToken)
        {
            return await PlaceBidAsync(dto, bidderId, bidderUsername, isAutoBid: false, cancellationToken);
        }

        public async Task<BidDto> PlaceBidAsync(PlaceBidDto dto, Guid bidderId, string bidderUsername, bool isAutoBid, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Placing bid for auction {AuctionId} by bidder {Bidder} (IsAutoBid: {IsAutoBid})",
                dto.AuctionId, bidderUsername, isAutoBid);

            return await ProcessBidWithLock(dto, bidderId, bidderUsername, isAutoBid, cancellationToken);
        }

        #region Private Helper Methods

        private async Task<BidDto> ProcessBidWithLock(
            PlaceBidDto dto,
            Guid bidderId,
            string bidderUsername,
            bool isAutoBid,
            CancellationToken ct)
        {
            var lockKey = BidLockKeys.ForAuction(dto.AuctionId);
            await using var lockHandle = await _distributedLock.TryAcquireAsync(
                lockKey,
                TimeSpan.FromSeconds(BidDefaults.BidLockTimeoutSeconds),
                ct);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for auction {AuctionId}", dto.AuctionId);
                return CreateRejectedBid(dto, bidderId, bidderUsername, "Another bid is being processed. Please try again.");
            }

            return await ExecuteBidTransaction(dto, bidderId, bidderUsername, isAutoBid, ct);
        }

        private async Task<BidDto> ExecuteBidTransaction(
            PlaceBidDto dto,
            Guid bidderId,
            string bidderUsername,
            bool isAutoBid,
            CancellationToken ct)
        {
            var highestBid = await _repository.GetHighestBidForAuctionAsync(dto.AuctionId, ct);
            var currentHighBid = highestBid?.Amount ?? 0;

            if (!IsValidBidIncrement(dto.Amount, currentHighBid, out var incrementError))
                return CreateBidTooLow(dto, bidderId, bidderUsername, incrementError);

            var bid = CreateBid(dto, bidderId, bidderUsername, highestBid, isAutoBid);
            var createdBid = await _repository.CreateAsync(bid, ct);

            var saveResult = await SaveBidWithRaceConditionHandling(dto, bidderId, bidderUsername, ct);
            if (saveResult != null)
                return saveResult;

            _logger.LogInformation("Bid {BidId} placed for auction {AuctionId} with status {Status}",
                createdBid.Id, dto.AuctionId, bid.Status);

            return _mapper.Map<BidDto>(createdBid);
        }

        private bool IsValidBidIncrement(decimal amount, decimal currentHighBid, out string errorMessage)
        {
            if (BidIncrement.IsValidBidAmount(amount, currentHighBid))
            {
                errorMessage = string.Empty;
                return true;
            }

            var minimumNextBid = BidIncrement.GetMinimumNextBid(currentHighBid);
            _logger.LogWarning(
                "Bid amount {Amount} does not meet minimum increment. Current high: {CurrentHigh}, Minimum next: {MinimumNext}",
                amount, currentHighBid, minimumNextBid);

            errorMessage = BidIncrement.GetIncrementErrorMessage(amount, currentHighBid);
            return false;
        }

        private Bid CreateBid(PlaceBidDto dto, Guid bidderId, string bidderUsername, Bid? highestBid, bool isAutoBid)
        {
            var bid = Bid.Create(
                dto.AuctionId,
                bidderId,
                bidderUsername,
                dto.Amount,
                _dateTime.UtcNow);

            if (highestBid == null || dto.Amount > highestBid.Amount)
            {
                bid.Accept(
                    highestBid?.Amount,
                    highestBid?.BidderId,
                    highestBid?.BidderUsername,
                    isAutoBid);
            }
            else
            {
                bid.MarkAsTooLow();
            }

            return bid;
        }

        private async Task<BidDto?> SaveBidWithRaceConditionHandling(
            PlaceBidDto dto,
            Guid bidderId,
            string bidderUsername,
            CancellationToken ct)
        {
            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
                return null;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                when (ex.InnerException?.Message.Contains("duplicate") == true ||
                      ex.InnerException?.Message.Contains("unique") == true)
            {
                _logger.LogWarning(
                    "Duplicate bid detected for auction {AuctionId} at amount {Amount}. Race condition handled.",
                    dto.AuctionId, dto.Amount);

                return CreateRejectedBid(dto, bidderId, bidderUsername,
                    "This bid amount was just placed by another bidder. Please try a higher amount.");
            }
        }

        private BidDto CreateRejectedBid(PlaceBidDto dto, Guid bidderId, string bidderUsername, string errorMessage)
        {
            return new BidDto
            {
                AuctionId = dto.AuctionId,
                BidderId = bidderId,
                BidderUsername = bidderUsername,
                Amount = dto.Amount,
                BidTime = _dateTime.UtcNow,
                Status = BidStatus.Rejected.ToString(),
                ErrorMessage = errorMessage
            };
        }

        private BidDto CreateBidTooLow(PlaceBidDto dto, Guid bidderId, string bidderUsername, string errorMessage)
        {
            return new BidDto
            {
                AuctionId = dto.AuctionId,
                BidderId = bidderId,
                BidderUsername = bidderUsername,
                Amount = dto.Amount,
                BidTime = _dateTime.UtcNow,
                Status = BidStatus.TooLow.ToString(),
                ErrorMessage = errorMessage
            };
        }

        #endregion

        public async Task<List<BidDto>> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting bids for auction {AuctionId}", auctionId);
            var bids = await _repository.GetBidsByAuctionIdAsync(auctionId, cancellationToken);
            return _mapper.Map<List<BidDto>>(bids);
        }

        public async Task<List<BidDto>> GetBidsForBidderAsync(string bidderUsername, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting bids for bidder {Bidder}", bidderUsername);
            var bids = await _repository.GetBidsByBidderUsernameAsync(bidderUsername, cancellationToken);
            return _mapper.Map<List<BidDto>>(bids);
        }
    }

    public static class BidLockKeys
    {
        public static string ForAuction(Guid auctionId) => $"auction-bid:{auctionId}";
    }
}
