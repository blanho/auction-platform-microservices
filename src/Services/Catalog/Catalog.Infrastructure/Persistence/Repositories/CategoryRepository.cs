#nullable enable
using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Extensions;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;

namespace Catalog.Infrastructure.Persistence.Repositories;

public class CategoryRepository : EfRepository<Category, CatalogDbContext>, ICategoryRepository
{
    protected override IQueryable<Category> Query => base.Query
        .Include(category => category.SubCategories.Where(child => !child.IsDeleted));

    public CategoryRepository(CatalogDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
        : base(context, dateTime, auditContext)
    {
    }

    public Task<bool> HasChildrenAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Categories.AnyAsync(x => !x.IsDeleted && x.ParentCategoryId == id, cancellationToken);

    public override async Task<PaginatedResult<Category>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Context.Categories
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name);

        return await query
            .ThenBy(entity => entity.Id)
            .ToPaginatedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Context.Categories
            .Where(x => !x.IsDeleted && x.Slug == slug)
            .Include(x => x.SubCategories.Where(s => !s.IsDeleted))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default) =>
        GetAllAsync(cancellationToken: cancellationToken);

    public async Task<List<Category>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = Context.Categories.Where(x => !x.IsDeleted);
        if (!includeInactive)
            query = query.Where(x => x.IsActive);

        return await query.AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetRootCategoriesWithChildrenAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Categories
            .Where(x => !x.IsDeleted && x.IsActive && x.ParentCategoryId == null)
            .Include(x => x.SubCategories.Where(s => !s.IsDeleted && s.IsActive))
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new List<Category>();

        return await Context.Categories
            .Where(x => !x.IsDeleted && idList.Contains(x.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = Context.Categories.Where(x => x.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);
        return await query.AnyAsync(cancellationToken);
    }
}
