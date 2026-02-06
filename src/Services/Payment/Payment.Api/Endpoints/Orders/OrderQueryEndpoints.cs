using Carter;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Application.Constants;
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

    private static async Task<Results<Ok<OrderDto>, NotFound, BadRequest<ProblemDetails>>> GetById(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<OrderDto>, NotFound, BadRequest<ProblemDetails>>> GetByAuctionId(
        Guid auctionId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderByAuctionIdQuery(auctionId);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return TypedResults.NotFound();

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<IReadOnlyList<OrderDto>>, BadRequest<ProblemDetails>>> GetByBuyer(
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

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append(TotalCountHeader, result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append(PageHeader, result.Value.Page.ToString());
        httpContext.Response.Headers.Append(PageSizeHeader, result.Value.PageSize.ToString());

        return TypedResults.Ok(result.Value.Items);
    }

    private static async Task<Results<Ok<IReadOnlyList<OrderDto>>, BadRequest<ProblemDetails>>> GetBySeller(
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

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append(TotalCountHeader, result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append(PageHeader, result.Value.Page.ToString());
        httpContext.Response.Headers.Append(PageSizeHeader, result.Value.PageSize.ToString());

        return TypedResults.Ok(result.Value.Items);
    }

    private static async Task<Results<Ok<IReadOnlyList<OrderDto>>, BadRequest<ProblemDetails>, UnauthorizedHttpResult>> GetMyPurchases(
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return TypedResults.Unauthorized();

        var query = new GetOrdersByBuyerQuery(
            username, 
            Page: page > 0 ? page : PaginationDefaults.DefaultPage, 
            PageSize: pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append(TotalCountHeader, result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append(PageHeader, result.Value.Page.ToString());
        httpContext.Response.Headers.Append(PageSizeHeader, result.Value.PageSize.ToString());

        return TypedResults.Ok(result.Value.Items);
    }

    private static async Task<Results<Ok<IReadOnlyList<OrderDto>>, BadRequest<ProblemDetails>, UnauthorizedHttpResult>> GetMySales(
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var username = httpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return TypedResults.Unauthorized();

        var query = new GetOrdersBySellerQuery(
            username, 
            Page: page > 0 ? page : PaginationDefaults.DefaultPage, 
            PageSize: pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append(TotalCountHeader, result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append(PageHeader, result.Value.Page.ToString());
        httpContext.Response.Headers.Append(PageSizeHeader, result.Value.PageSize.ToString());

        return TypedResults.Ok(result.Value.Items);
    }

    private static async Task<Results<Ok<IReadOnlyList<OrderDto>>, BadRequest<ProblemDetails>>> GetAll(
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

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append(TotalCountHeader, result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append(PageHeader, result.Value.Page.ToString());
        httpContext.Response.Headers.Append(PageSizeHeader, result.Value.PageSize.ToString());

        return TypedResults.Ok(result.Value.Items);
    }

    private static async Task<Results<Ok<OrderStatsDto>, BadRequest<ProblemDetails>>> GetStats(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetOrderStatsQuery();
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        return TypedResults.Ok(result.Value);
    }
}

public record GetAllOrdersFilter(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);
