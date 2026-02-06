namespace BuildingBlocks.Application.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);
    
    Task<PaginatedResult<T>> GetPagedAsync(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("AddRangeAsync not implemented for this repository");
    
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("UpdateRangeAsync not implemented for this repository");
    
    Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("DeleteRangeAsync not implemented for this repository");
}