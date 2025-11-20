using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Services
{
    public class AuctionService : IAuctionService
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionService(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<AuctionDto>> GetAllAuctionsAsync(CancellationToken cancellationToken)
        {
            return await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ProjectTo<AuctionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<AuctionDto> GetAuctionByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return auction == null ? null : _mapper.Map<AuctionDto>(auction);
        }

        public async Task<AuctionDto> CreateAuctionAsync(CreateAuctionDto dto, string seller, CancellationToken cancellationToken)
        {
            var auction = _mapper.Map<Auction>(dto);
            auction.Seller = seller;

            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return null;
            }

            return _mapper.Map<AuctionDto>(auction);
        }

        public async Task<bool> UpdateAuctionAsync(Guid id, UpdateAuctionDto dto, CancellationToken cancellationToken)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (auction == null)
            {
                return false;
            }

            auction.Item.Make = dto.Make ?? auction.Item.Make;
            auction.Item.Model = dto.Model ?? auction.Item.Model;
            auction.Item.Color = dto.Color ?? auction.Item.Color;
            auction.Item.Mileage = dto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = dto.Year ?? auction.Item.Year;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task<bool> DeleteAuctionAsync(Guid id, CancellationToken cancellationToken)
        {
            var auction = await _context.Auctions.FindAsync(new object[] { id }, cancellationToken);

            if (auction == null)
            {
                return false;
            }

            _context.Auctions.Remove(auction);

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
