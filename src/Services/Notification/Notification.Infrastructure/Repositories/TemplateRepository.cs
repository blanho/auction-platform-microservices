using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly NotificationDbContext _context;

    public TemplateRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        return await _context.Templates
            .FirstOrDefaultAsync(t => t.Key == key.ToLowerInvariant(), ct);
    }

    public async Task<List<NotificationTemplate>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.Templates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<List<NotificationTemplate>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Templates
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task AddAsync(NotificationTemplate template, CancellationToken ct = default)
    {
        await _context.Templates.AddAsync(template, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NotificationTemplate template, CancellationToken ct = default)
    {
        _context.Templates.Update(template);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await _context.Templates.FindAsync(new object[] { id }, ct);
        if (template != null)
        {
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync(ct);
        }
    }
}
