#nullable enable
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AuctionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Common.Core.Interfaces;
using Common.Core.Constants;

namespace AuctionService.Infrastructure.Repositories
{
    public class FlashSaleRepository : IFlashSaleRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public FlashSaleRepository(AuctionDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<FlashSale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.FlashSales
                .Where(f => !f.IsDeleted)
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<FlashSale?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.FlashSales
                .Where(f => !f.IsDeleted)
                .Include(f => f.Items)
                .ThenInclude(i => i.Auction)
                .ThenInclude(a => a!.Item)
                .AsSplitQuery()
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<List<FlashSale>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var query = _context.FlashSales.Where(f => !f.IsDeleted);
            if (!includeInactive)
                query = query.Where(f => f.IsActive);
            return await query
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<FlashSale>> GetActiveFlashSalesAsync(CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            return await _context.FlashSales
                .Where(f => !f.IsDeleted && f.IsActive && f.StartTime <= now && f.EndTime > now)
                .Include(f => f.Items)
                .ThenInclude(i => i.Auction)
                .ThenInclude(a => a!.Item)
                .AsSplitQuery()
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<FlashSale?> GetCurrentFlashSaleAsync(CancellationToken cancellationToken = default)
        {
            var now = _dateTime.UtcNow;
            return await _context.FlashSales
                .Where(f => !f.IsDeleted && f.IsActive && f.StartTime <= now && f.EndTime > now)
                .Include(f => f.Items)
                .ThenInclude(i => i.Auction)
                .ThenInclude(a => a!.Item)
                .AsSplitQuery()
                .OrderBy(f => f.DisplayOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<FlashSale> AddAsync(FlashSale flashSale, CancellationToken cancellationToken = default)
        {
            flashSale.CreatedAt = _dateTime.UtcNow;
            flashSale.CreatedBy = SystemGuids.System;
            flashSale.IsDeleted = false;
            await _context.FlashSales.AddAsync(flashSale, cancellationToken);
            return flashSale;
        }

        public Task UpdateAsync(FlashSale flashSale, CancellationToken cancellationToken = default)
        {
            flashSale.UpdatedAt = _dateTime.UtcNow;
            _context.FlashSales.Update(flashSale);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var flashSale = await GetByIdAsync(id, cancellationToken);
            if (flashSale != null)
            {
                flashSale.IsDeleted = true;
                flashSale.DeletedAt = _dateTime.UtcNow;
                flashSale.DeletedBy = SystemGuids.System;
                _context.FlashSales.Update(flashSale);
            }
        }

        public async Task<FlashSaleItem> AddItemAsync(FlashSaleItem item, CancellationToken cancellationToken = default)
        {
            item.Id = Guid.NewGuid();
            item.AddedAt = _dateTime.UtcNow;
            await _context.FlashSaleItems.AddAsync(item, cancellationToken);
            return item;
        }

        public async Task RemoveItemAsync(Guid flashSaleId, Guid auctionId, CancellationToken cancellationToken = default)
        {
            var item = await _context.FlashSaleItems
                .FirstOrDefaultAsync(i => i.FlashSaleId == flashSaleId && i.AuctionId == auctionId, cancellationToken);
            if (item != null)
            {
                _context.FlashSaleItems.Remove(item);
            }
        }
    }
}
