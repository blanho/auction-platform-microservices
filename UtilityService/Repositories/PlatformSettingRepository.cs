using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.Domain.Entities;
using UtilityService.Interfaces;

namespace UtilityService.Repositories;

public class PlatformSettingRepository : IPlatformSettingRepository
{
    private readonly UtilityDbContext _context;

    public PlatformSettingRepository(UtilityDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<PlatformSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task<List<PlatformSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PlatformSetting>> GetByCategoryAsync(SettingCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.PlatformSettings
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
        return await _context.PlatformSettings.AnyAsync(s => s.Key == key, cancellationToken);
    }
}
