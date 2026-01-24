using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Infrastructure.Repository.Specifications;

namespace BuildingBlocks.Infrastructure.Repository;

public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PaginatedResult<T>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Specification pattern not implemented for this repository");
    
    Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Specification pattern not implemented for this repository");
    
    Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("Specification pattern not implemented for this repository");
}
