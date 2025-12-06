using Microsoft.EntityFrameworkCore;

namespace Common.Repository.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> specification) where T : class
    {
        var query = inputQuery;

        query = query.Where(specification.Criteria);

        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }

    public static async Task<int> CountAsync<T>(IQueryable<T> inputQuery, ISpecification<T> specification, CancellationToken cancellationToken = default) where T : class
    {
        return await inputQuery.Where(specification.Criteria).CountAsync(cancellationToken);
    }
}
