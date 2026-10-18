using BuildingBlocks.Application.Abstractions;
using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface ITemplateRepository
{
    Task<NotificationTemplate?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task<List<NotificationTemplate>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    Task<PaginatedResult<NotificationTemplate>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default);

    Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);

    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
