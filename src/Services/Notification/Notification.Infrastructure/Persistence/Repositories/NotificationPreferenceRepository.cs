using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Persistence.Repositories;

public class NotificationPreferenceRepository : EfRepository<NotificationPreference, NotificationDbContext>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(
        NotificationDbContext context, IDateTimeProvider clock, IAuditContext auditContext)
        : base(context, clock, auditContext)
    {
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await Query
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }
}
