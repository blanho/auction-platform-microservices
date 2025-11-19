using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface ITemplateRepository
{

    Task<NotificationTemplate?> GetByKeyAsync(string key, CancellationToken ct = default);

    Task<List<NotificationTemplate>> GetAllActiveAsync(CancellationToken ct = default);

    Task<List<NotificationTemplate>> GetAllAsync(CancellationToken ct = default);

    Task AddAsync(NotificationTemplate template, CancellationToken ct = default);

    Task UpdateAsync(NotificationTemplate template, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
