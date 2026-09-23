using Catalog.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Catalog.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> HasChildrenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Category>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<List<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetRootCategoriesWithChildrenAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
