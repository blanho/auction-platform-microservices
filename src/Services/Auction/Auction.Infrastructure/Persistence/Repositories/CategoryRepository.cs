#nullable enable
using Auctions.Domain.Entities;
using Auctions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;

namespace Auctions.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AuctionDbContext _context;
        private readonly IDateTimeProvider _dateTime;

        public CategoryRepository(AuctionDbContext context, IDateTimeProvider dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        public async Task<PaginatedResult<Category>> GetPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Categories
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<Category>(items, totalCount, page, pageSize);
        }

        public async Task<Category> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories
                .Where(x => !x.IsDeleted)
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            return category ?? throw new KeyNotFoundException($"Category with ID {id} not found");
        }

        public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(x => !x.IsDeleted && x.Slug == slug)
                .Include(x => x.Items.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Category>> GetCategoriesWithCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(x => !x.IsDeleted && x.IsActive)
                .Include(x => x.Items.Where(i => !i.IsDeleted && i.Auction != null && !i.Auction.IsDeleted))
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.Where(x => !x.IsDeleted && x.Slug == slug);
            
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
        {
            category.CreatedAt = _dateTime.UtcNow;
            category.CreatedBy = SystemGuids.System;
            category.IsDeleted = false;
            
            await _context.Categories.AddAsync(category, cancellationToken);
            
            return category;
        }

        public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
        {
            category.UpdatedAt = _dateTime.UtcNow;
            _context.Categories.Update(category);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
        {
            category.IsDeleted = true;
            category.DeletedAt = _dateTime.UtcNow;
            _context.Categories.Update(category);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await GetByIdAsync(id, cancellationToken);
            category.IsDeleted = true;
            category.DeletedAt = _dateTime.UtcNow;
            _context.Categories.Update(category);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(x => !x.IsDeleted)
                .AnyAsync(x => x.Id == id, cancellationToken);
        }
    }
}

