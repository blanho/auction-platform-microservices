using Carter;
using Common.Core.Constants;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands.CreateOrder;
using PaymentService.Application.Commands.ProcessPayment;
using PaymentService.Application.Commands.ShipOrder;
using PaymentService.Application.Commands.UpdateOrderStatus;
using PaymentService.Application.DTOs;
using PaymentService.Application.Queries.GetOrderByAuctionId;
using PaymentService.Application.Queries.GetOrderById;
using PaymentService.Application.Queries.GetOrdersByBuyer;
using PaymentService.Application.Queries.GetOrdersBySeller;

namespace PaymentService.API.Endpoints;

public class OrderEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetOrderById")
            .WithSummary("Get order by ID");

        group.MapGet("/auction/{auctionId:guid}", GetByAuctionId)
            .WithName("GetOrderByAuctionId")
            .WithSummary("Get order by auction ID");

        group.MapGet("/buyer/{username}", GetByBuyer)
            .WithName("GetOrdersByBuyer")
            .WithSummary("Get orders by buyer username");

        group.MapGet("/seller/{username}", GetBySeller)
            .WithName("GetOrdersBySeller")
            .WithSummary("Get orders by seller username");

        group.MapPost("/", Create)
            .WithName("CreateOrder")
            .WithSummary("Create a new order");

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateOrder")
            .WithSummary("Update an order");

        group.MapPost("/{id:guid}/payment", ProcessPayment)
            .WithName("ProcessOrderPayment")
            .WithSummary("Process payment for an order");

        group.MapPost("/{id:guid}/ship", ShipOrder)
            .WithName("ShipOrder")
            .WithSummary("Mark order as shipped");
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

    private static async Task<Results<Created<OrderDto>, BadRequest<ProblemDetails>>> Create(
        CreateOrderDto dto,
        IMediator mediator,
        ILogger<OrderEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            dto.AuctionId,
            dto.BuyerId,
            dto.BuyerUsername,
            dto.SellerId,
            dto.SellerUsername,
            dto.ItemTitle,
            dto.WinningBid,
            dto.ShippingCost,
            dto.PlatformFee,
            dto.ShippingAddress,
            dto.BuyerNotes
        );

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        logger.LogInformation("Order created: {OrderId} for auction {AuctionId}", result.Value.Id, result.Value.AuctionId);

        return TypedResults.Created($"/api/v1/orders/{result.Value.Id}", result.Value);
    }

    private static async Task<Results<Ok<OrderDto>, BadRequest<ProblemDetails>>> Update(
        Guid id,
        UpdateOrderDto dto,
        IMediator mediator,
        ILogger<OrderEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrderStatusCommand(
            id,
            dto.Status,
            dto.PaymentStatus,
            dto.PaymentTransactionId,
            dto.ShippingAddress,
            dto.TrackingNumber,
            dto.ShippingCarrier,
            dto.BuyerNotes,
            dto.SellerNotes
        );

        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        logger.LogInformation("Order updated: {OrderId}", result.Value.Id);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<OrderDto>, BadRequest<ProblemDetails>>> ProcessPayment(
        Guid id,
        ProcessPaymentDto dto,
        IMediator mediator,
        ILogger<OrderEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand(id, dto.PaymentMethod, dto.ExternalTransactionId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        logger.LogInformation("Payment processed for order: {OrderId}", result.Value.Id);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<OrderDto>, BadRequest<ProblemDetails>>> ShipOrder(
        Guid id,
        UpdateShippingDto dto,
        IMediator mediator,
        ILogger<OrderEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new ShipOrderCommand(id, dto.TrackingNumber, dto.ShippingCarrier, dto.SellerNotes);
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return TypedResults.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        logger.LogInformation("Order shipped: {OrderId}", result.Value.Id);

        return TypedResults.Ok(result.Value);
    }
}
