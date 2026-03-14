using Auctions.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Category>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetCategoriesWithCountAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetByIdsForUpdateAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

