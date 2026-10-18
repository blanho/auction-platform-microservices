#nullable enable
using BuildingBlocks.Infrastructure.Repository;
using Catalog.Domain.Entities;
using Catalog.Application.Filtering;
using Catalog.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Catalog.Infrastructure.Persistence.Repositories;

public class BrandRepository : EfRepository<Brand, CatalogDbContext>, IBrandRepository
{
    public BrandRepository(CatalogDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
        : base(context, dateTime, auditContext)
    {
    }

    public Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        FirstOrDefaultAsync(brand => brand.Slug == slug, cancellationToken);

    public async Task<PaginatedResult<Brand>> GetPagedAsync(BrandQueryParams parameters, CancellationToken cancellationToken = default)
    {
        var query = BrandQueries.Filter(Context.Brands.AsNoTracking(), parameters);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await BrandQueries.Page(query, parameters).ToListAsync(cancellationToken);
        return new PaginatedResult<Brand>(items, totalCount, parameters.Page, parameters.PageSize);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Brands.Where(b => b.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(b => b.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }
}
