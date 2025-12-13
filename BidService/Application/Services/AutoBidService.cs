using BidService.Application.DTOs;
using BidService.Application.Interfaces;
using BidService.Domain.Entities;
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

        public async Task<AutoBidDto?> CreateAutoBidAsync(CreateAutoBidDto dto, string bidder, CancellationToken cancellationToken = default)
        {
            var existingAutoBid = await _autoBidRepository.GetActiveAutoBidAsync(dto.AuctionId, bidder);

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
                Bidder = bidder,
                MaxAmount = dto.MaxAmount,
                CurrentBidAmount = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _autoBidRepository.AddAsync(autoBid);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Auto-bid created for auction {AuctionId} by {Bidder} with max amount {MaxAmount}",
                dto.AuctionId, bidder, dto.MaxAmount);

            return MapToDto(autoBid);
        }

        public async Task<AutoBidDto?> GetAutoBidByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id);
            return autoBid != null ? MapToDto(autoBid) : null;
        }

        public async Task<AutoBidDto?> GetActiveAutoBidAsync(Guid auctionId, string bidder, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetActiveAutoBidAsync(auctionId, bidder);
            return autoBid != null ? MapToDto(autoBid) : null;
        }

        public async Task<List<AutoBidDto>> GetAutoBidsByBidderAsync(string bidder, CancellationToken cancellationToken = default)
        {
            var autoBids = await _autoBidRepository.GetAutoBidsByBidderAsync(bidder);
            return autoBids.Select(MapToDto).ToList();
        }

        public async Task<AutoBidDto?> UpdateAutoBidAsync(Guid id, UpdateAutoBidDto dto, string bidder, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id);

            if (autoBid == null || autoBid.Bidder != bidder)
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

        public async Task<bool> CancelAutoBidAsync(Guid id, string bidder, CancellationToken cancellationToken = default)
        {
            var autoBid = await _autoBidRepository.GetByIdAsync(id);

            if (autoBid == null || autoBid.Bidder != bidder)
                return false;

            autoBid.IsActive = false;
            await _autoBidRepository.UpdateAsync(autoBid);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Auto-bid {Id} cancelled by {Bidder}", id, bidder);
            return true;
        }

        public async Task ProcessAutoBidsForAuctionAsync(Guid auctionId, int currentHighBid, CancellationToken cancellationToken = default)
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
                secondHighestMax + GetBidIncrement(secondHighestMax)
            );

            if (newBidAmount <= currentHighBid)
            {
                newBidAmount = currentHighBid + GetBidIncrement(currentHighBid);
            }

            if (newBidAmount <= highestAutoBid.MaxAmount)
            {
                try
                {
                    var bidDto = new PlaceBidDto { AuctionId = auctionId, Amount = newBidAmount };
                    await _bidService.PlaceBidAsync(bidDto, highestAutoBid.Bidder, cancellationToken);

                    highestAutoBid.CurrentBidAmount = newBidAmount;
                    highestAutoBid.LastBidAt = DateTime.UtcNow;
                    await _autoBidRepository.UpdateAsync(highestAutoBid);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Auto-bid placed for auction {AuctionId} by {Bidder} for {Amount}",
                        auctionId, highestAutoBid.Bidder, newBidAmount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process auto-bid for auction {AuctionId}", auctionId);
                }
            }
        }

        private static int GetBidIncrement(int currentBid)
        {
            return currentBid switch
            {
                < 100 => 10,
                < 500 => 25,
                < 1000 => 50,
                < 5000 => 100,
                < 10000 => 250,
                < 50000 => 500,
                _ => 1000
            };
        }

        private static AutoBidDto MapToDto(AutoBid autoBid)
        {
            return new AutoBidDto(
                autoBid.Id,
                autoBid.AuctionId,
                autoBid.Bidder,
                autoBid.MaxAmount,
                autoBid.CurrentBidAmount,
                autoBid.IsActive,
                autoBid.CreatedAt,
                autoBid.LastBidAt
            );
        }
    }
}
