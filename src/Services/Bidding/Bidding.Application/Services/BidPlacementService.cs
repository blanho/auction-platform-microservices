using Bidding.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.Services
{
    public class BidPlacementService : IBidService
    {
        private readonly IBidRepository _repository;
        private readonly IAuctionBidValidator _auctionValidator;
        private readonly ILogger<BidPlacementService> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IBidWriter _bidWriter;
        private readonly IDistributedLock _distributedLock;
        private readonly IAuctionBidLock _auctionBidLock;

        public BidPlacementService(
            IBidRepository repository,
            IAuctionBidValidator auctionValidator,
            ILogger<BidPlacementService> logger,
            IDateTimeProvider dateTime,
            IBidWriter bidWriter,
            IDistributedLock distributedLock,
            IAuctionBidLock auctionBidLock)
        {
            _repository = repository;
            _auctionValidator = auctionValidator;
            _logger = logger;
            _dateTime = dateTime;
            _bidWriter = bidWriter;
            _distributedLock = distributedLock;
            _auctionBidLock = auctionBidLock;
        }

        public async Task<BidDto> PlaceBidAsync(PlaceBidDto dto, Guid bidderId, string bidderUsername, CancellationToken cancellationToken = default)
        {
            return await PlaceBidAsync(dto, bidderId, bidderUsername, isAutoBid: false, cancellationToken);
        }

        public async Task<BidDto> PlaceBidAsync(PlaceBidDto dto, Guid bidderId, string bidderUsername, bool isAutoBid, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Placing bid for auction {AuctionId} by bidder {Bidder} (IsAutoBid: {IsAutoBid})",
                dto.AuctionId, bidderUsername, isAutoBid);

            return await ProcessBidWithLock(dto, bidderId, bidderUsername, isAutoBid, cancellationToken);
        }

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
                TimeSpan.FromSeconds(BidDefaults.BidLockExpirySeconds),
                ct);

            if (lockHandle == null)
            {
                _logger.LogWarning("Failed to acquire lock for auction {AuctionId}", dto.AuctionId);
                return CreateRejectedBid(dto, bidderId, bidderUsername, "Another bid is being processed. Please try again.");
            }

            return await ExecuteBidWithinAuctionLock(dto, bidderId, bidderUsername, isAutoBid, ct);
        }

        private async Task<BidDto> ExecuteBidWithinAuctionLock(
            PlaceBidDto dto,
            Guid bidderId,
            string bidderUsername,
            bool isAutoBid,
            CancellationToken ct)
        {
            return await _auctionBidLock.ExecuteAsync(
                dto.AuctionId,
                async lockCt =>
                {
                    var validation = await _auctionValidator.ValidateAsync(dto, bidderId, bidderUsername, lockCt);
                    if (!validation.IsValid)
                        return CreateRejectedBid(dto, bidderId, bidderUsername, validation.ErrorMessage);

                    var highestBid = await _repository.GetHighestBidForAuctionAsync(dto.AuctionId, lockCt);
                    var currentHighBid = highestBid?.Amount ?? 0;

                    if (!IsValidBidIncrement(dto.Amount, currentHighBid, out var incrementError))
                        return CreateBidTooLow(dto, bidderId, bidderUsername, incrementError);

                    var bid = await CreateAndSaveBid(dto, bidderId, bidderUsername, isAutoBid, highestBid, validation.ReservePrice, lockCt);
                    if (bid.Status == BidStatus.Rejected.ToString())
                        return bid;

                    _logger.LogInformation("Bid {BidId} placed for auction {AuctionId} with status {Status}",
                        bid.Id, dto.AuctionId, bid.Status);

                    return bid;
                },
                ct);
        }

        private bool IsValidBidIncrement(decimal amount, decimal currentHighBid, out string errorMessage)
        {
            if (currentHighBid == 0)
            {
                errorMessage = amount > 0 ? string.Empty : "Bid amount must be greater than zero.";
                return amount > 0;
            }

            var minimumNextBid = BidIncrementHelper.GetMinimumNextBid(currentHighBid);
            if (amount >= minimumNextBid)
            {
                errorMessage = string.Empty;
                return true;
            }

            var increment = BidIncrementHelper.GetIncrement(currentHighBid);
            _logger.LogWarning(
                "Bid amount {Amount} does not meet minimum increment. Current high: {CurrentHigh}, Minimum next: {MinimumNext}",
                amount, currentHighBid, minimumNextBid);
            errorMessage = $"Bid must be at least ${minimumNextBid:N2}. Minimum increment is ${increment:N2} for bids at this level.";
            return false;
        }

        private Bid CreateBid(PlaceBidDto dto, Guid bidderId, string bidderUsername, Bid? highestBid, bool isAutoBid, decimal reservePrice)
        {
            var bid = Bid.Create(
                dto.AuctionId,
                bidderId,
                bidderUsername,
                dto.Amount,
                _dateTime.UtcNow);

            if (highestBid != null && dto.Amount <= highestBid.Amount)
            {
                bid.MarkAsTooLow();
                return bid;
            }

            if (reservePrice > 0 && dto.Amount < reservePrice)
            {
                bid.AcceptBelowReserve(
                    highestBid?.Amount,
                    highestBid?.BidderId,
                    highestBid?.BidderUsername,
                    isAutoBid);
            }
            else
            {
                bid.Accept(
                    highestBid?.Amount,
                    highestBid?.BidderId,
                    highestBid?.BidderUsername,
                    isAutoBid);
            }

            return bid;
        }

        private async Task<BidDto?> SaveBidWithRaceConditionHandling(
            Bid bid,
            PlaceBidDto dto,
            Guid bidderId,
            string bidderUsername,
            CancellationToken ct)
        {
            try
            {
                await _bidWriter.CreateAsync(bid, ct);
                return null;
            }
            catch (Exception ex) when (IsUniqueConstraintViolation(ex))
            {
                _logger.LogWarning(
                    ex,
                    "Duplicate bid detected for auction {AuctionId} at amount {Amount}. Race condition handled.",
                    dto.AuctionId, dto.Amount);

                return CreateRejectedBid(dto, bidderId, bidderUsername,
                    "This bid amount was just placed by another bidder. Please try a higher amount.");
            }
        }

        private static bool IsUniqueConstraintViolation(Exception ex)
        {
            var inner = ex.InnerException;
            if (inner == null) return false;
            var message = inner.Message;
            return message.Contains("23505", StringComparison.OrdinalIgnoreCase)
                || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase);
        }

        private BidDto CreateRejectedBid(PlaceBidDto dto, Guid bidderId, string bidderUsername, string errorMessage)
        {
            return CreateUnsuccessfulBid(dto, bidderId, bidderUsername, BidStatus.Rejected, errorMessage);
        }

        private BidDto CreateBidTooLow(PlaceBidDto dto, Guid bidderId, string bidderUsername, string errorMessage)
        {
            return CreateUnsuccessfulBid(dto, bidderId, bidderUsername, BidStatus.TooLow, errorMessage);
        }

        private BidDto CreateUnsuccessfulBid(
            PlaceBidDto dto,
            Guid bidderId,
            string bidderUsername,
            BidStatus status,
            string errorMessage)
        {
            return new BidDto
            {
                AuctionId = dto.AuctionId,
                BidderId = bidderId,
                BidderUsername = bidderUsername,
                Amount = dto.Amount,
                BidTime = _dateTime.UtcNow,
                Status = status.ToString(),
                ErrorMessage = errorMessage
            };
        }

        private async Task<BidDto> CreateAndSaveBid(PlaceBidDto dto, Guid bidderId, string bidderUsername, bool isAutoBid, Bid? highestBid, decimal reservePrice, CancellationToken ct)
        {
            var bid = CreateBid(dto, bidderId, bidderUsername, highestBid, isAutoBid, reservePrice);

            var raceConditionResult = await SaveBidWithRaceConditionHandling(bid, dto, bidderId, bidderUsername, ct);
            if (raceConditionResult != null)
                return raceConditionResult;

            return bid.ToDto();
        }

        public async Task<PaginatedResult<BidDto>> GetBidsForAuctionAsync(BidQueryParams queryParams, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting bids for auction {AuctionId}, Page: {Page}",
                queryParams.Filter.AuctionId, queryParams.Page);

            var result = await _repository.GetBidsForAuctionPagedAsync(queryParams, cancellationToken);

            return new PaginatedResult<BidDto>(
                result.Items.ToDtoList(),
                result.TotalCount,
                result.Page,
                result.PageSize);
        }

        public async Task<PaginatedResult<BidDto>> GetBidsForBidderAsync(BidQueryParams queryParams, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting bids for bidder {Bidder}, Page: {Page}",
                queryParams.Filter.BidderUsername, queryParams.Page);

            var result = await _repository.GetBidsForBidderPagedAsync(queryParams, cancellationToken);

            return new PaginatedResult<BidDto>(
                result.Items.ToDtoList(),
                result.TotalCount,
                result.Page,
                result.PageSize);
        }
    }
}
