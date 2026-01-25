using UnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

namespace Bidding.Application.Services
{
    public class BidPlacementService : IBidService
    {
        private readonly IBidRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<BidPlacementService> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEventPublisher _eventPublisher;
        private readonly UnitOfWork _unitOfWork;
        private readonly IDistributedLock _distributedLock;
        private readonly IAuctionGrpcClient _auctionGrpcClient;

        public BidPlacementService(
            IBidRepository repository,
            IMapper mapper,
            ILogger<BidPlacementService> logger,
            IDateTimeProvider dateTime,
            IEventPublisher eventPublisher,
            UnitOfWork unitOfWork,
            IDistributedLock distributedLock,
            IAuctionGrpcClient auctionGrpcClient)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
            _eventPublisher = eventPublisher;
            _unitOfWork = unitOfWork;
            _distributedLock = distributedLock;
            _auctionGrpcClient = auctionGrpcClient;
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
            var auctionValidation = await ValidateAuctionForBid(dto, bidderUsername, bidderId, ct);
            if (auctionValidation != null)
                return auctionValidation;

            var bidIncrementValidation = await ValidateBidIncrement(dto, bidderId, bidderUsername, ct);
            if (bidIncrementValidation != null)
                return bidIncrementValidation;

            var bid = await CreateAndSaveBid(dto, bidderId, bidderUsername, isAutoBid, ct);
            if (bid.Status == BidStatus.Rejected.ToString())
                return bid;

            await CheckAndExtendAuctionIfNeeded(dto.AuctionId, ct);

            _logger.LogInformation("Bid {BidId} placed for auction {AuctionId} with status {Status}",
                bid.Id, dto.AuctionId, bid.Status);

            return bid;
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

        private async Task CheckAndExtendAuctionIfNeeded(Guid auctionId, CancellationToken ct)
        {
            try
            {
                var auctionDetails = await _auctionGrpcClient.GetAuctionDetailsAsync(auctionId, ct);
                if (auctionDetails == null) return;

                var timeRemaining = auctionDetails.EndTime - _dateTime.UtcNow;
                if (timeRemaining <= TimeSpan.FromMinutes(BidDefaults.AntiSnipeThresholdMinutes) && 
                    timeRemaining > TimeSpan.Zero)
                {
                    var newEndTime = auctionDetails.EndTime.AddMinutes(BidDefaults.AntiSnipeExtensionMinutes);
                    var result = await _auctionGrpcClient.ExtendAuctionAsync(auctionId, newEndTime, ct);
                    
                    if (result.Success)
                    {
                        _logger.LogInformation(
                            "Auction {AuctionId} extended to {NewEndTime} due to anti-snipe rule",
                            auctionId,
                            result.NewEndTime);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check/extend auction {AuctionId} for anti-snipe", auctionId);
            }
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

        private async Task<BidDto?> ValidateAuctionForBid(PlaceBidDto dto, string bidderUsername, Guid bidderId, CancellationToken ct)
        {
            var validationResult = await _auctionGrpcClient.ValidateAuctionForBidAsync(
                dto.AuctionId,
                bidderUsername,
                dto.Amount,
                ct);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Auction validation failed for {AuctionId}: {ErrorCode} - {ErrorMessage}",
                    dto.AuctionId,
                    validationResult.ErrorCode,
                    validationResult.ErrorMessage);

                return CreateRejectedBid(dto, bidderId, bidderUsername, validationResult.ErrorMessage);
            }

            return null;
        }

        private async Task<BidDto?> ValidateBidIncrement(PlaceBidDto dto, Guid bidderId, string bidderUsername, CancellationToken ct)
        {
            var highestBid = await _repository.GetHighestBidForAuctionAsync(dto.AuctionId, ct);
            var currentHighBid = highestBid?.Amount ?? 0;

            if (!IsValidBidIncrement(dto.Amount, currentHighBid, out var incrementError))
                return CreateBidTooLow(dto, bidderId, bidderUsername, incrementError);

            return null;
        }

        private async Task<BidDto> CreateAndSaveBid(PlaceBidDto dto, Guid bidderId, string bidderUsername, bool isAutoBid, CancellationToken ct)
        {
            var highestBid = await _repository.GetHighestBidForAuctionAsync(dto.AuctionId, ct);
            var bid = CreateBid(dto, bidderId, bidderUsername, highestBid, isAutoBid);
            
            var createdBid = await _repository.CreateAsync(bid, ct);

            var raceConditionResult = await SaveBidWithRaceConditionHandling(dto, bidderId, bidderUsername, ct);
            if (raceConditionResult != null)
                return raceConditionResult;

            return createdBid.ToDto();
        }

        #endregion

        public async Task<List<BidDto>> GetBidsForAuctionAsync(Guid auctionId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting bids for auction {AuctionId}", auctionId);
            var bids = await _repository.GetBidsByAuctionIdAsync(auctionId, cancellationToken);
            return bids.ToDtoList();
        }

        public async Task<List<BidDto>> GetBidsForBidderAsync(string bidderUsername, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting bids for bidder {Bidder}", bidderUsername);
            var bids = await _repository.GetBidsByBidderUsernameAsync(bidderUsername, cancellationToken);
            return bids.ToDtoList();
        }
    }

    public static class BidLockKeys
    {
        public static string ForAuction(Guid auctionId) => $"auction-bid:{auctionId}";
    }
}
