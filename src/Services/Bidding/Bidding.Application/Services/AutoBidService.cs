using Bidding.Domain.Enums;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Locking;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.Extensions.Logging;

namespace Bidding.Application.Services
{
    public class AutoBidService : IAutoBidService
    {
        private readonly IAutoBidRepository _autoBidRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBidService _bidService;
        private readonly IDistributedLock _distributedLock;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILogger<AutoBidService> _logger;
        private readonly IAuctionGrpcClient _auctionGrpcClient;

        public AutoBidService(
            IAutoBidRepository autoBidRepository,
            IUnitOfWork unitOfWork,
            IBidService bidService,
            IDistributedLock distributedLock,
            IDateTimeProvider dateTime,
            ILogger<AutoBidService> logger,
            IAuctionGrpcClient auctionGrpcClient)
        {
            _autoBidRepository = autoBidRepository;
            _unitOfWork = unitOfWork;
            _bidService = bidService;
            _distributedLock = distributedLock;
            _dateTime = dateTime;
            _logger = logger;
            _auctionGrpcClient = auctionGrpcClient;
        }

        public async Task ProcessAutoBidsForAuctionAsync(Guid auctionId, decimal currentHighBid, CancellationToken cancellationToken = default)
        {
            var lockKey = AutoBidLockKeys.ForAuction(auctionId);
            await using var lockHandle = await _distributedLock.AcquireAsync(
                lockKey,
                expiry: TimeSpan.FromSeconds(BidDefaults.AutoBidLockExpirySeconds),
                wait: TimeSpan.FromSeconds(BidDefaults.AutoBidLockWaitSeconds),
                cancellationToken);

            if (lockHandle is null)
            {
                _logger.LogWarning(
                    "Failed to acquire auto-bid lock for auction {AuctionId}. Requesting message retry.",
                    auctionId);
                throw new TimeoutException($"Timed out acquiring the auto-bid lock for auction {auctionId}.");
            }

            await ProcessAutoBidsWithLock(auctionId, currentHighBid, cancellationToken);
        }

        private async Task ProcessAutoBidsWithLock(Guid auctionId, decimal currentHighBid, CancellationToken cancellationToken)
        {
            var auctionDetails = await _auctionGrpcClient.GetAuctionDetailsAsync(auctionId, cancellationToken);

            if (!IsAuctionEligibleForAutoBidProcessing(auctionDetails, auctionId))
                return;

            var eligibleAutoBids = await GetEligibleAutoBids(auctionId, currentHighBid, cancellationToken);

            if (eligibleAutoBids.Count == 0)
                return;

            await PlaceOptimalAutoBid(eligibleAutoBids, auctionId, currentHighBid, cancellationToken);
        }

        private bool IsAuctionEligibleForAutoBidProcessing(AuctionDetails? auctionDetails, Guid auctionId)
        {
            if (auctionDetails == null || auctionDetails.Status != BidDefaults.AuctionStatuses.Live || auctionDetails.EndTime <= _dateTime.UtcNow)
            {
                _logger.LogInformation(
                    "Skipping auto-bid processing for auction {AuctionId} - auction not eligible",
                    auctionId);
                return false;
            }

            return true;
        }

        private async Task<List<AutoBid>> GetEligibleAutoBids(Guid auctionId, decimal currentHighBid, CancellationToken cancellationToken)
        {
            var activeAutoBids = await _autoBidRepository.GetActiveAutoBidsForAuctionAsync(auctionId, cancellationToken);

            return activeAutoBids
                .Where(ab => ab.MaxAmount > currentHighBid)
                .OrderByDescending(ab => ab.MaxAmount)
                .ThenBy(ab => ab.CreatedAt)
                .ToList();
        }

        private async Task PlaceOptimalAutoBid(List<AutoBid> eligibleAutoBids, Guid auctionId, decimal currentHighBid, CancellationToken cancellationToken)
        {
            var highestAutoBid = eligibleAutoBids[0];
            var secondHighestMax = eligibleAutoBids.Count > 1 ? eligibleAutoBids[1].MaxAmount : currentHighBid;

            var newBidAmount = CalculateOptimalBidAmount(highestAutoBid.MaxAmount, secondHighestMax, currentHighBid);

            if (newBidAmount > highestAutoBid.MaxAmount)
                return;

            await ExecuteAutoBid(highestAutoBid, auctionId, newBidAmount, cancellationToken);
        }

        private static decimal CalculateOptimalBidAmount(decimal maxAmount, decimal secondHighestMax, decimal currentHighBid)
        {
            var optimalBid = Math.Min(
                maxAmount,
                secondHighestMax + BidIncrementHelper.GetIncrement(secondHighestMax)
            );

            if (optimalBid <= currentHighBid)
            {
                optimalBid = currentHighBid + BidIncrementHelper.GetIncrement(currentHighBid);
            }

            return optimalBid;
        }

        private async Task ExecuteAutoBid(AutoBid autoBid, Guid auctionId, decimal amount, CancellationToken cancellationToken)
        {
            var bidDto = new PlaceBidDto { AuctionId = auctionId, Amount = amount };
            var bid = await _bidService.PlaceBidAsync(bidDto, autoBid.UserId, autoBid.Username, isAutoBid: true, cancellationToken);

            if (!IsAcceptedBid(bid))
            {
                _logger.LogWarning(
                    "Auto-bid for auction {AuctionId} by {Username} was not accepted. Status: {Status}, Reason: {Reason}",
                    auctionId,
                    autoBid.Username,
                    bid.Status,
                    bid.ErrorMessage);
                return;
            }

            autoBid.RecordBid(amount);
            await _autoBidRepository.UpdateAsync(autoBid);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-bid placed for auction {AuctionId} by {Username} for {Amount}",
                auctionId, autoBid.Username, amount);
        }

        private static bool IsAcceptedBid(BidDto bid)
        {
            return bid.Status is nameof(BidStatus.Accepted) or nameof(BidStatus.AcceptedBelowReserve);
        }
    }
}
