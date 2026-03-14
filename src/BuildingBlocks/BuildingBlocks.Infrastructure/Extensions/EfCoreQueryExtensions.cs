using System.Linq.Expressions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Application.Paging;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class EfCoreQueryExtensions
{
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedPageSize = Math.Clamp(pageSize, 1, PaginationDefaults.MaxPageSize);
        var normalizedPage = Math.Max(page, 1);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, totalCount, normalizedPage, normalizedPageSize);
    }

    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        QueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        await query.ToPaginatedResultAsync(parameters.Page, parameters.PageSize, cancellationToken);

    public static async Task<PaginatedResult<TResult>> ToPaginatedResultAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        var normalizedPageSize = Math.Clamp(pageSize, 1, PaginationDefaults.MaxPageSize);
        var normalizedPage = Math.Max(page, 1);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TResult>(items, totalCount, normalizedPage, normalizedPageSize);
    }

    public static async Task<PaginatedResult<TResult>> ToPaginatedResultAsync<T, TResult>(
        this IQueryable<T> query,
        QueryParameters parameters,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default) =>
        await query.ToPaginatedResultAsync(parameters.Page, parameters.PageSize, selector, cancellationToken);
}
