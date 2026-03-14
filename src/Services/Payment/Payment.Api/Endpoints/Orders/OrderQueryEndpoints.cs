using Carter;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTOs;
using Payment.Application.Features.Orders.GetAllOrders;
using Payment.Application.Features.Orders.GetOrderByAuctionId;
using Payment.Application.Features.Orders.GetOrderById;
using Payment.Application.Features.Orders.GetOrdersByBuyer;
using Payment.Application.Features.Orders.GetOrdersBySeller;
using Payment.Application.Features.Orders.GetOrderStats;
using Payment.Domain.Enums;
using BuildingBlocks.Application.Abstractions;

namespace Payment.Api.Endpoints.Orders;

public class OrderQueryEndpoints : ICarterModule
{
    private const string TotalCountHeader = "X-Total-Count";
    private const string PageHeader = "X-Page";
    private const string PageSizeHeader = "X-Page-Size";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetOrderById")
            .WithSummary("Get order by ID")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.View));

        group.MapGet("/auction/{auctionId:guid}", GetByAuctionId)
            .WithName("GetOrderByAuctionId")
            .WithSummary("Get order by auction ID")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.View));

        group.MapGet("/buyer/{username}", GetByBuyer)
            .WithName("GetOrdersByBuyer")
            .WithSummary("Get orders by buyer username")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.View));

        group.MapGet("/seller/{username}", GetBySeller)
            .WithName("GetOrdersBySeller")
            .WithSummary("Get orders by seller username")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.View));

        group.MapGet("/buyer/me", GetMyPurchases)
            .WithName("GetMyPurchases")
            .WithSummary("Get current user's purchases")
            .RequireAuthorization();

        group.MapGet("/seller/me", GetMySales)
            .WithName("GetMySales")
            .WithSummary("Get current user's sales")
            .RequireAuthorization();

        group.MapGet("/", GetAll)
            .WithName("GetAllOrders")
            .WithSummary("Get all orders with filtering and pagination")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.View));

        group.MapGet("/stats", GetStats)
            .WithName("GetOrderStats")
            .WithSummary("Get order statistics")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.View));
    }

    private static async Task<IResult> GetById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> GetByAuctionId(
        Guid auctionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByAuctionIdQuery(auctionId);
        var result = await mediator.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static async Task<IResult> GetByBuyer(
        string username,
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersByBuyerQuery(
            username, 
            Page: page > 0 ? page : PaginationDefaults.DefaultPage, 
            PageSize: pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        return result.ToApiResult(paginatedResult =>
        {
            AppendPaginationHeaders(httpContext, paginatedResult.TotalCount, paginatedResult.Page, paginatedResult.PageSize);
            return Results.Ok(paginatedResult.Items);
        });
    }

    private static async Task<IResult> GetBySeller(
        string username,
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersBySellerQuery(
            username, 
            Page: page > 0 ? page : PaginationDefaults.DefaultPage, 
            PageSize: pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        return result.ToApiResult(paginatedResult =>
        {
            AppendPaginationHeaders(httpContext, paginatedResult.TotalCount, paginatedResult.Page, paginatedResult.PageSize);
            return Results.Ok(paginatedResult.Items);
        });
    }

    private static async Task<IResult> GetMyPurchases(
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);

        var query = new GetOrdersByBuyerQuery(
            username, 
            Page: page > 0 ? page : PaginationDefaults.DefaultPage, 
            PageSize: pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        return result.ToApiResult(paginatedResult =>
        {
            AppendPaginationHeaders(httpContext, paginatedResult.TotalCount, paginatedResult.Page, paginatedResult.PageSize);
            return Results.Ok(paginatedResult.Items);
        });
    }

    private static async Task<IResult> GetMySales(
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var username = UserHelper.GetUsername(httpContext.User);

        var query = new GetOrdersBySellerQuery(
            username, 
            Page: page > 0 ? page : PaginationDefaults.DefaultPage, 
            PageSize: pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        return result.ToApiResult(paginatedResult =>
        {
            AppendPaginationHeaders(httpContext, paginatedResult.TotalCount, paginatedResult.Page, paginatedResult.PageSize);
            return Results.Ok(paginatedResult.Items);
        });
    }

    private static async Task<IResult> GetAll(
        [AsParameters] GetAllOrdersFilter filter,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetAllOrdersQuery(
            filter.Page > 0 ? filter.Page : PaginationDefaults.DefaultPage,
            filter.PageSize > 0 ? filter.PageSize : PaginationDefaults.DefaultPageSize,
            filter.Search,
            filter.Status,
            filter.FromDate,
            filter.ToDate);

        var result = await mediator.Send(query, cancellationToken);

        return result.ToApiResult(paginatedResult =>
        {
            AppendPaginationHeaders(httpContext, paginatedResult.TotalCount, paginatedResult.Page, paginatedResult.PageSize);
            return Results.Ok(paginatedResult.Items);
        });
    }

    private static async Task<IResult> GetStats(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderStatsQuery();
        var result = await mediator.Send(query, cancellationToken);

        return result.ToOkResult();
    }

    private static void AppendPaginationHeaders(HttpContext httpContext, int totalCount, int page, int pageSize)
    {
        httpContext.Response.Headers.Append(TotalCountHeader, totalCount.ToString());
        httpContext.Response.Headers.Append(PageHeader, page.ToString());
        httpContext.Response.Headers.Append(PageSizeHeader, pageSize.ToString());
    }
}
