using Microsoft.EntityFrameworkCore;
using MassTransit;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Application.Abstractions;
using Analytics.Infrastructure.Persistence;
using Analytics.Domain.Entities;
using Analytics.Domain.Enums;
using Analytics.Application.Interfaces;

namespace Analytics.Infrastructure.Repositories;

public class PlatformSettingRepository : IPlatformSettingRepository
{
    private readonly AnalyticsDbContext _context;

    public PlatformSettingRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<PlatformSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task<PaginatedResult<PlatformSetting>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.PlatformSettings
            .AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<PlatformSetting>(items, totalCount, page, pageSize);
    }

    public async Task<List<PlatformSetting>> GetByCategoryAsync(SettingCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
            .AsNoTracking()
            .Where(s => s.Category == category)
            .OrderBy(s => s.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlatformSetting> AddAsync(PlatformSetting setting, CancellationToken cancellationToken = default)
    {
        _context.PlatformSettings.Add(setting);
        await _context.SaveChangesAsync(cancellationToken);
        return setting;
    }

    public async Task UpdateAsync(PlatformSetting setting, CancellationToken cancellationToken = default)
    {
        _context.PlatformSettings.Update(setting);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var setting = await _context.PlatformSettings.FindAsync([id], cancellationToken);
        if (setting != null)
        {
            _context.PlatformSettings.Remove(setting);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
            .AsNoTracking()
            .AnyAsync(s => s.Key == key, cancellationToken);
    }
}
