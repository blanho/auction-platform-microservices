using Auctions.Domain.Entities;
using BuildingBlocks.Application.Constants;

namespace Auctions.Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Brand?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Brand>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<List<Brand>> GetBrandsWithItemCountAsync(CancellationToken cancellationToken = default);
        Task<List<Brand>> GetFeaturedBrandsAsync(int count = PaginationDefaults.DefaultPageSize, CancellationToken cancellationToken = default);
        Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default);
        Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

