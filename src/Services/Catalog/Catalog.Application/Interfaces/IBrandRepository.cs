using Catalog.Domain.Entities;
using Catalog.Application.Filtering;

namespace Catalog.Application.Interfaces;

public interface IBrandRepository
{
    Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Brand>> GetPagedAsync(BrandQueryParams parameters, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default);
    Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
