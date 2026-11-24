using System.Linq.Expressions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Paging;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class EfCoreQueryExtensions
{
    public static Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        query.ToPaginatedResultAsync(QueryParameters.Create(page, pageSize), cancellationToken);

    public static Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        QueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        ReadPageAsync(query, query.ApplyPaging(parameters), parameters, cancellationToken);

    public static Task<PaginatedResult<TResult>> ToPaginatedResultAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) =>
        query.ToPaginatedResultAsync(QueryParameters.Create(page, pageSize), selector, cancellationToken);

    public static Task<PaginatedResult<TResult>> ToPaginatedResultAsync<T, TResult>(
        this IQueryable<T> query,
        QueryParameters parameters,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) =>
        ReadPageAsync(query, query.ApplyPaging(parameters).Select(selector), parameters, cancellationToken);

    private static async Task<PaginatedResult<TResult>> ReadPageAsync<T, TResult>(
        IQueryable<T> query,
        IQueryable<TResult> pageQuery,
        QueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await pageQuery.ToListAsync(cancellationToken);
        return new PaginatedResult<TResult>(items, totalCount, parameters.Page, parameters.PageSize);
    }
}
