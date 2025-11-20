using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;

namespace AuctionService.Infrastructure.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly IAuctionRepository _repository;
        private readonly IMapper _mapper;

        public AuctionService(IAuctionRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<AuctionDto>> GetAllAuctionsAsync(CancellationToken cancellationToken)
        {
            var auctions = await _repository.GetAllAsync(cancellationToken);
            return auctions.Select(a => _mapper.Map<AuctionDto>(a)).ToList();
        }

        public async Task<AuctionDto> GetAuctionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var auction = await _repository.GetByIdAsync(id, cancellationToken);
            return auction == null ? new AuctionDto() : _mapper.Map<AuctionDto>(auction);
        }

        public async Task<AuctionDto> CreateAuctionAsync(CreateAuctionDto dto, string seller, CancellationToken cancellationToken)
        {
            var auction = _mapper.Map<Domain.Entities.Auction>(dto);
            auction.Seller = seller;
            var createdAuction = await _repository.CreateAsync(auction, cancellationToken);
            return _mapper.Map<AuctionDto>(createdAuction);
        }

        public async Task<bool> UpdateAuctionAsync(Guid id, UpdateAuctionDto dto, CancellationToken cancellationToken)
        {
            var auction = await _repository.GetByIdAsync(id, cancellationToken);
            if (auction == null) return false;
            auction.Item.Make = dto.Make ?? auction.Item.Make;
            auction.Item.Model = dto.Model ?? auction.Item.Model;
            auction.Item.Color = dto.Color ?? auction.Item.Color;
            auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = dto.Year ?? auction.Item.Year;
            await _repository.UpdateAsync(auction, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAuctionAsync(Guid id, CancellationToken cancellationToken)
        {
            var exists = await _repository.ExistsAsync(id, cancellationToken);
            if (!exists) return false;
            await _repository.DeleteAsync(id, cancellationToken);
            return true;
        }
    }
}
