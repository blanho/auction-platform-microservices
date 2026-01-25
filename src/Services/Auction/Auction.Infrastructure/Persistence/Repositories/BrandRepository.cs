#nullable enable
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Constants;

namespace Auctions.Infrastructure.Persistence.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public BrandRepository(AuctionDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .Where(b => !b.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<Brand?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .Where(b => !b.IsDeleted)
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Auction)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .Where(b => !b.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Slug == slug, cancellationToken);
        }

        public async Task<List<Brand>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Brands.Where(b => !b.IsDeleted);
            if (!includeInactive)
                query = query.Where(b => b.IsActive);
            return await query
                .AsNoTracking()
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Brand>> GetBrandsWithItemCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .Where(b => !b.IsDeleted && b.IsActive)
                .Include(b => b.Items.Where(i => !i.IsDeleted && i.Auction != null && !i.Auction.IsDeleted))
                .AsNoTracking()
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Brand>> GetFeaturedBrandsAsync(int count = PaginationDefaults.DefaultPageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Brands
                .Where(b => !b.IsDeleted && b.IsActive && b.IsFeatured)
                .AsNoTracking()
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Brands.Where(b => !b.IsDeleted && b.Slug == slug);
            if (excludeId.HasValue)
                query = query.Where(b => b.Id != excludeId.Value);
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            brand.CreatedAt = _dateTime.UtcNow;
            brand.CreatedBy = SystemGuids.System;
            brand.IsDeleted = false;
            await _context.Brands.AddAsync(brand, cancellationToken);
            return brand;
        }

        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            brand.UpdatedAt = _dateTime.UtcNow;
            _context.Brands.Update(brand);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var brand = await GetByIdAsync(id, cancellationToken);
            if (brand != null)
            {
                brand.IsDeleted = true;
                brand.DeletedAt = _dateTime.UtcNow;
                brand.DeletedBy = SystemGuids.System;
                _context.Brands.Update(brand);
            }
        }
    }
}

