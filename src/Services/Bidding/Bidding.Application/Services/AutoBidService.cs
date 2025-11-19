using Microsoft.Extensions.Logging;
using UnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

namespace Bidding.Application.Services
{
    public class AutoBidService : IAutoBidService
    {
        private readonly IAutoBidRepository _autoBidRepository;
        private readonly UnitOfWork _unitOfWork;
        private readonly IBidService _bidService;
        private readonly IDistributedLock _distributedLock;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILogger<AutoBidService> _logger;

        public AutoBidService(
            IAutoBidRepository autoBidRepository,
            UnitOfWork unitOfWork,
            IBidService bidService,
            IDistributedLock distributedLock,
            IDateTimeProvider dateTime,
            ILogger<AutoBidService> logger)
        {
            _autoBidRepository = autoBidRepository;
            _unitOfWork = unitOfWork;
            _bidService = bidService;
            _distributedLock = distributedLock;
            _dateTime = dateTime;
            _logger = logger;
        }

        public async Task<AutoBidDto?> CreateAutoBidAsync(CreateAutoBidDto dto, Guid userId, string username, CancellationToken cancellationToken = default)
        {
            var existingAutoBid = await _autoBidRepository.GetActiveAutoBidAsync(dto.AuctionId, userId, cancellationToken);

            if (existingAutoBid != null)
            {
                existingAutoBid.UpdateMaxAmount(dto.MaxAmount);
                existingAutoBid.Activate();
                await _autoBidRepository.UpdateAsync(existingAutoBid, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return MapToDto(existingAutoBid);
            }

            var autoBid = AutoBid.Create(dto.AuctionId, userId, username, dto.MaxAmount);

            await _autoBidRepository.CreateAsync(autoBid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-bid created for auction {AuctionId} by {Username} with max amount {MaxAmount}",
                dto.AuctionId, username, dto.MaxAmount);

            return MapToDto(autoBid);
        }

        public async Task<AutoBidDto?> GetAutoBidByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id, cancellationToken);
            return autoBid != null ? MapToDto(autoBid) : null;
        }

        public async Task<AutoBidDto?> GetActiveAutoBidAsync(Guid auctionId, Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetActiveAutoBidAsync(auctionId, userId, cancellationToken);
            return autoBid != null ? MapToDto(autoBid) : null;
        }

        public async Task<List<AutoBidDto>> GetAutoBidsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBids = await _autoBidRepository.GetAutoBidsByUserAsync(userId, null, 1, 100, cancellationToken);
            return autoBids.Select(MapToDto).ToList();
        }

        public async Task<AutoBidDto?> UpdateAutoBidAsync(Guid id, UpdateAutoBidDto dto, Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id, cancellationToken);

            if (autoBid == null || autoBid.UserId != userId)
                return null;

            autoBid.UpdateMaxAmount(dto.MaxAmount);
            if (dto.IsActive.HasValue)
            {
                if (dto.IsActive.Value)
                    autoBid.Activate();
                else
                    autoBid.Deactivate();
            }

            await _autoBidRepository.UpdateAsync(autoBid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToDto(autoBid);
        }

        public async Task<bool> CancelAutoBidAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id, cancellationToken);

            if (autoBid == null || autoBid.UserId != userId)
                return false;

            autoBid.Deactivate();
            await _autoBidRepository.UpdateAsync(autoBid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Auto-bid {Id} cancelled by user {UserId}", id, userId);
            return true;
        }

        public async Task ProcessAutoBidsForAuctionAsync(Guid auctionId, decimal currentHighBid, CancellationToken cancellationToken = default)
        {
            var lockKey = $"autobid:auction:{auctionId}";
            await using var lockHandle = await _distributedLock.AcquireAsync(
                lockKey,
                expiry: TimeSpan.FromSeconds(BidDefaults.AutoBidLockExpirySeconds),
                wait: TimeSpan.FromSeconds(BidDefaults.AutoBidLockWaitSeconds),
                cancellationToken);
            
            if (lockHandle is null)
            {
                _logger.LogWarning(
                    "Failed to acquire auto-bid lock for auction {AuctionId}. Another process is handling auto-bids.",
                    auctionId);
                return;
            }
            
            try
            {
                var activeAutoBids = await _autoBidRepository.GetActiveAutoBidsForAuctionAsync(auctionId, cancellationToken);
                var eligibleAutoBids = activeAutoBids
                    .Where(ab => ab.MaxAmount > currentHighBid)
                    .OrderByDescending(ab => ab.MaxAmount)
                    .ThenBy(ab => ab.CreatedAt)
                    .ToList();

                if (eligibleAutoBids.Count == 0)
                {
                    return;
                }

                var highestAutoBid = eligibleAutoBids.First();
                var secondHighestMax = eligibleAutoBids.Count > 1 ? eligibleAutoBids[1].MaxAmount : currentHighBid;

                var newBidAmount = Math.Min(
                    highestAutoBid.MaxAmount,
                    secondHighestMax + BidIncrement.GetIncrement(secondHighestMax)
                );

                if (newBidAmount <= currentHighBid)
                {
                    newBidAmount = currentHighBid + BidIncrement.GetIncrement(currentHighBid);
                }

                if (newBidAmount <= highestAutoBid.MaxAmount)
                {
                    var bidDto = new PlaceBidDto { AuctionId = auctionId, Amount = newBidAmount };
                    await _bidService.PlaceBidAsync(bidDto, highestAutoBid.UserId, highestAutoBid.Username, isAutoBid: true, cancellationToken);

                    highestAutoBid.RecordBid(newBidAmount);
                    await _autoBidRepository.UpdateAsync(highestAutoBid);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Auto-bid placed for auction {AuctionId} by {Username} for {Amount}",
                        auctionId, highestAutoBid.Username, newBidAmount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing auto-bids for auction {AuctionId}", auctionId);
                throw;
            }
        }

        private static AutoBidDto MapToDto(AutoBid autoBid)
        {
            return new AutoBidDto(
                autoBid.Id,
                autoBid.AuctionId,
                autoBid.UserId,
                autoBid.Username,
                autoBid.MaxAmount,
                autoBid.CurrentBidAmount,
                autoBid.IsActive,
                autoBid.CreatedAt,
                autoBid.LastBidAt
            );
        }
    }
}

