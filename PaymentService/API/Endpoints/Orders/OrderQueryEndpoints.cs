using Carter;
using Common.Core.Authorization;
using Common.Core.Constants;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Queries.GetOrderByAuctionId;
using PaymentService.Application.Queries.GetOrderById;
using PaymentService.Application.Queries.GetOrdersByBuyer;
using PaymentService.Application.Queries.GetOrdersBySeller;

namespace PaymentService.API.Endpoints.Orders;

public class OrderQueryEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetOrderById")
            .WithSummary("Get order by ID")
            .RequireAuthorization($"Permission:{Permissions.Orders.ViewOwn}");

        group.MapGet("/auction/{auctionId:guid}", GetByAuctionId)
            .WithName("GetOrderByAuctionId")
            .WithSummary("Get order by auction ID")
            .RequireAuthorization($"Permission:{Permissions.Orders.ViewOwn}");

        group.MapGet("/buyer/{username}", GetByBuyer)
            .WithName("GetOrdersByBuyer")
            .WithSummary("Get orders by buyer username")
            .RequireAuthorization($"Permission:{Permissions.Orders.ViewOwn}");

        group.MapGet("/seller/{username}", GetBySeller)
            .WithName("GetOrdersBySeller")
            .WithSummary("Get orders by seller username")
            .RequireAuthorization($"Permission:{Permissions.Orders.ViewOwn}");
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

    private static async Task<Results<Ok<IEnumerable<OrderDto>>, BadRequest<ProblemDetails>>> GetByBuyer(
        string username,
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersByBuyerQuery(username, page > 0 ? page : PaginationDefaults.DefaultPage, pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append("X-Page", result.Value.Page.ToString());
        httpContext.Response.Headers.Append("X-Page-Size", result.Value.PageSize.ToString());

        return TypedResults.Ok(result.Value.Items);
    }

    private static async Task<Results<Ok<IEnumerable<OrderDto>>, BadRequest<ProblemDetails>>> GetBySeller(
        string username,
        int page,
        int pageSize,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var query = new GetOrdersBySellerQuery(username, page > 0 ? page : PaginationDefaults.DefaultPage, pageSize > 0 ? pageSize : PaginationDefaults.DefaultPageSize);
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        httpContext.Response.Headers.Append("X-Page", result.Value.Page.ToString());
        httpContext.Response.Headers.Append("X-Page-Size", result.Value.PageSize.ToString());

        return TypedResults.Ok(result.Value.Items);
    }
}
