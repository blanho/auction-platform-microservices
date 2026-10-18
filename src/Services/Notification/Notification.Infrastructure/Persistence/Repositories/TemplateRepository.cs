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

    public Task<NotificationTemplate?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        FirstOrDefaultAsync(template => template.Key == key.ToLowerInvariant(), cancellationToken);

    public async Task<List<NotificationTemplate>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public override async Task<PaginatedResult<NotificationTemplate>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Query
            .AsNoTracking()
            .OrderBy(t => t.Name);
        return await query
            .ThenBy(entity => entity.Id)
            .ToPaginatedResultAsync(page, pageSize, cancellationToken);
    }

    Task ITemplateRepository.AddAsync(NotificationTemplate template, CancellationToken cancellationToken) =>
        CreateAsync(template, cancellationToken);

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

    public override async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdForUpdateAsync(id, cancellationToken);
        if (template != null)
        {
            Context.Templates.Remove(template);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
