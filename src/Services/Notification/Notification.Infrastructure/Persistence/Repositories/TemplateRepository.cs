using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Persistence.Repositories;

public class TemplateRepository : EfRepository<NotificationTemplate, NotificationDbContext>, ITemplateRepository
{
    public TemplateRepository(
        NotificationDbContext context, IDateTimeProvider clock, IAuditContext auditContext)
        : base(context, clock, auditContext)
    {
    }

    public async Task<NotificationTemplate?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        return await Query
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Key == key.ToLowerInvariant(), ct);
    }

    public async Task<List<NotificationTemplate>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await Query
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public override async Task<PaginatedResult<NotificationTemplate>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = Query
            .AsNoTracking()
            .OrderBy(t => t.Name);
        return await query
            .ThenBy(entity => entity.Id)
            .ToPaginatedResultAsync(page, pageSize, ct);
    }

    Task ITemplateRepository.AddAsync(NotificationTemplate template, CancellationToken ct) =>
        CreateAsync(template, ct);

    public override async Task<NotificationTemplate> CreateAsync(
        NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        await base.CreateAsync(template, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public override async Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(template, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public override async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await GetByIdForUpdateAsync(id, ct);
        if (template != null)
        {
            Context.Templates.Remove(template);
            await Context.SaveChangesAsync(ct);
        }
    }
}
