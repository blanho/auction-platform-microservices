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
        private readonly IAuctionGrpcClient _auctionGrpcClient;

        public AutoBidService(
            IAutoBidRepository autoBidRepository,
            UnitOfWork unitOfWork,
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

        public async Task<AutoBidDto?> CreateAutoBidAsync(CreateAutoBidDto dto, Guid userId, string username, CancellationToken cancellationToken = default)
        {
            var auctionDetails = await _auctionGrpcClient.GetAuctionDetailsAsync(dto.AuctionId, cancellationToken);
            
            if (!IsAuctionEligibleForAutoBid(auctionDetails, username, dto.AuctionId))
                return null;

            var existingAutoBid = await _autoBidRepository.GetActiveAutoBidAsync(dto.AuctionId, userId, cancellationToken);

            if (existingAutoBid != null)
                return await UpdateExistingAutoBid(existingAutoBid, dto.MaxAmount, cancellationToken);

            return await CreateNewAutoBid(dto, userId, username, cancellationToken);
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

            if (!IsAuthorizedToModifyAutoBid(autoBid, userId))
                return null;

            UpdateAutoBidSettings(autoBid!, dto);

            await _autoBidRepository.UpdateAsync(autoBid!, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return MapToDto(autoBid!);
        }

        public async Task<bool> CancelAutoBidAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id, cancellationToken);

            if (!IsAuthorizedToModifyAutoBid(autoBid, userId))
                return false;

            autoBid!.Deactivate();
            await _autoBidRepository.UpdateAsync(autoBid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Auto-bid {Id} cancelled by user {UserId}", id, userId);
            return true;
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
                    "Failed to acquire auto-bid lock for auction {AuctionId}. Another process is handling auto-bids.",
                    auctionId);
                return;
            }
            
            try
            {
                await ProcessAutoBidsWithLock(auctionId, currentHighBid, cancellationToken);
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

        #region Private Helper Methods

        private const string AuctionStatusActive = "Active";

        private bool IsAuctionEligibleForAutoBid(AuctionDetails? auctionDetails, string username, Guid auctionId)
        {
            if (auctionDetails == null)
            {
                _logger.LogWarning("Cannot create auto-bid for non-existent auction {AuctionId}", auctionId);
                return false;
            }

            if (auctionDetails.Status != AuctionStatusActive)
            {
                _logger.LogWarning("Cannot create auto-bid for inactive auction {AuctionId} (Status: {Status})",
                    auctionId, auctionDetails.Status);
                return false;
            }

            if (auctionDetails.SellerUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("User {Username} attempted to create auto-bid on own auction {AuctionId}",
                    username, auctionId);
                return false;
            }

            if (auctionDetails.EndTime <= _dateTime.UtcNow)
            {
                _logger.LogWarning("Cannot create auto-bid for ended auction {AuctionId}", auctionId);
                return false;
            }

            return true;
        }

        private async Task<AutoBidDto> UpdateExistingAutoBid(AutoBid existingAutoBid, decimal newMaxAmount, CancellationToken cancellationToken)
        {
            existingAutoBid.UpdateMaxAmount(newMaxAmount);
            existingAutoBid.Activate();
            await _autoBidRepository.UpdateAsync(existingAutoBid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToDto(existingAutoBid);
        }

        private async Task<AutoBidDto> CreateNewAutoBid(CreateAutoBidDto dto, Guid userId, string username, CancellationToken cancellationToken)
        {
            var autoBid = AutoBid.Create(dto.AuctionId, userId, username, dto.MaxAmount);

            await _autoBidRepository.CreateAsync(autoBid, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-bid created for auction {AuctionId} by {Username} with max amount {MaxAmount}",
                dto.AuctionId, username, dto.MaxAmount);

            return MapToDto(autoBid);
        }

        private static bool IsAuthorizedToModifyAutoBid(AutoBid? autoBid, Guid userId)
        {
            return autoBid != null && autoBid.UserId == userId;
        }

        private static void UpdateAutoBidSettings(AutoBid autoBid, UpdateAutoBidDto dto)
        {
            autoBid.UpdateMaxAmount(dto.MaxAmount);
            
            if (dto.IsActive.HasValue)
            {
                if (dto.IsActive.Value)
                    autoBid.Activate();
                else
                    autoBid.Deactivate();
            }
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
            if (auctionDetails == null || auctionDetails.Status != AuctionStatusActive || auctionDetails.EndTime <= _dateTime.UtcNow)
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
            var highestAutoBid = eligibleAutoBids.First();
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
                secondHighestMax + BidIncrement.GetIncrement(secondHighestMax)
            );

            if (optimalBid <= currentHighBid)
            {
                optimalBid = currentHighBid + BidIncrement.GetIncrement(currentHighBid);
            }

            return optimalBid;
        }

        private async Task ExecuteAutoBid(AutoBid autoBid, Guid auctionId, decimal amount, CancellationToken cancellationToken)
        {
            var bidDto = new PlaceBidDto { AuctionId = auctionId, Amount = amount };
            await _bidService.PlaceBidAsync(bidDto, autoBid.UserId, autoBid.Username, isAutoBid: true, cancellationToken);

            autoBid.RecordBid(amount);
            await _autoBidRepository.UpdateAsync(autoBid);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-bid placed for auction {AuctionId} by {Username} for {Amount}",
                auctionId, autoBid.Username, amount);
        }

        #endregion
    }

    public static class AutoBidLockKeys
    {
        public static string ForAuction(Guid auctionId) => $"autobid:auction:{auctionId}";
    }
}
