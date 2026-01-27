using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Repositories;

public class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly NotificationDbContext _context;

    public NotificationPreferenceRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<NotificationPreference> CreateAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        await _context.NotificationPreferences.AddAsync(preference, cancellationToken);
        return preference;
    }

    public async Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        _context.NotificationPreferences.Update(preference);
        await Task.CompletedTask;
    }
}
