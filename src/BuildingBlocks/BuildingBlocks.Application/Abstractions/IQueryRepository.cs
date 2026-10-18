using System.Linq.Expressions;
using BuildingBlocks.Domain.Entities;

namespace BuildingBlocks.Application.Abstractions;

public interface IQueryRepository<T> : IReadRepository<T> where T : AggregateRoot
{
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultForUpdateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<T>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<List<T>> GetByIdsForUpdateAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<PaginatedResult<T>> GetPagedAsync(
        Expression<Func<T, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<TResult>> GetPagedAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
