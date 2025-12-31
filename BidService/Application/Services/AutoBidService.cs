using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using BidService.Domain.Entities;
using BidService.Domain.ValueObjects;
using Common.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace BidService.Application.Services
{
    public class AutoBidService : IAutoBidService
    {
        private readonly IAutoBidRepository _autoBidRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBidService _bidService;
        private readonly ILogger<AutoBidService> _logger;

        public AutoBidService(
            IAutoBidRepository autoBidRepository,
            IUnitOfWork unitOfWork,
            IBidService bidService,
            ILogger<AutoBidService> logger)
        {
            _autoBidRepository = autoBidRepository;
            _unitOfWork = unitOfWork;
            _bidService = bidService;
            _logger = logger;
        }

        public async Task<AutoBidDto?> CreateAutoBidAsync(CreateAutoBidDto dto, Guid userId, string username, CancellationToken cancellationToken = default)
        {
            var existingAutoBid = await _autoBidRepository.GetActiveAutoBidAsync(dto.AuctionId, userId);

            if (existingAutoBid != null)
            {
                existingAutoBid.MaxAmount = dto.MaxAmount;
                existingAutoBid.IsActive = true;
                await _autoBidRepository.UpdateAsync(existingAutoBid);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return MapToDto(existingAutoBid);
            }

            var autoBid = new AutoBid
            {
                Id = Guid.NewGuid(),
                AuctionId = dto.AuctionId,
                UserId = userId,
                Username = username,
                MaxAmount = dto.MaxAmount,
                CurrentBidAmount = 0,
                IsActive = true
            };

            await _autoBidRepository.AddAsync(autoBid);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-bid created for auction {AuctionId} by {Username} with max amount {MaxAmount}",
                dto.AuctionId, username, dto.MaxAmount);

            return MapToDto(autoBid);
        }

        public async Task<AutoBidDto?> GetAutoBidByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id);
            return autoBid != null ? MapToDto(autoBid) : null;
        }

        public async Task<AutoBidDto?> GetActiveAutoBidAsync(Guid auctionId, Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetActiveAutoBidAsync(auctionId, userId);
            return autoBid != null ? MapToDto(autoBid) : null;
        }

        public async Task<List<AutoBidDto>> GetAutoBidsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBids = await _autoBidRepository.GetAutoBidsByUserAsync(userId);
            return autoBids.Select(MapToDto).ToList();
        }

        public async Task<AutoBidDto?> UpdateAutoBidAsync(Guid id, UpdateAutoBidDto dto, Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id);

            if (autoBid == null || autoBid.UserId != userId)
                return null;

            autoBid.MaxAmount = dto.MaxAmount;
            if (dto.IsActive.HasValue)
            {
                autoBid.IsActive = dto.IsActive.Value;
            }

            await _autoBidRepository.UpdateAsync(autoBid);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToDto(autoBid);
        }

        public async Task<bool> CancelAutoBidAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id);

            if (autoBid == null || autoBid.UserId != userId)
                return false;

            autoBid.IsActive = false;
            await _autoBidRepository.UpdateAsync(autoBid);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Auto-bid {Id} cancelled by user {UserId}", id, userId);
            return true;
        }

        public async Task ProcessAutoBidsForAuctionAsync(Guid auctionId, decimal currentHighBid, CancellationToken cancellationToken = default)
        {
            var activeAutoBids = await _autoBidRepository.GetActiveAutoBidsForAuctionAsync(auctionId);
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
                await _bidService.PlaceBidAsync(bidDto, highestAutoBid.UserId, highestAutoBid.Username, cancellationToken);

                highestAutoBid.CurrentBidAmount = newBidAmount;
                highestAutoBid.LastBidAt = DateTimeOffset.UtcNow;
                await _autoBidRepository.UpdateAsync(highestAutoBid);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Auto-bid placed for auction {AuctionId} by {Username} for {Amount}",
                    auctionId, highestAutoBid.Username, newBidAmount);
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
