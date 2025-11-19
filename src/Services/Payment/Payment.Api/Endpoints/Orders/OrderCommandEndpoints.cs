using Carter;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Features.Orders.Commands.CreateOrder;
using Payment.Application.Features.Orders.Commands.ProcessPayment;
using Payment.Application.Features.Orders.Commands.ShipOrder;
using Payment.Application.Features.Orders.Commands.UpdateOrderStatus;
using Payment.Application.DTOs;

namespace Payment.Api.Endpoints.Orders;

public class OrderCommandEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPost("/", Create)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Create));

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateOrder")
            .WithSummary("Update an order")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Cancel));

        group.MapPost("/{id:guid}/payment", ProcessPayment)
            .WithName("ProcessOrderPayment")
            .WithSummary("Process payment for an order")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Payments.Process));

        group.MapPost("/{id:guid}/ship", ShipOrder)
            .WithName("ShipOrder")
            .WithSummary("Mark order as shipped")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Cancel));
    }

    private static async Task<Results<Created<OrderDto>, BadRequest<ProblemDetails>>> Create(
        CreateOrderDto dto,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
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
        ILogger<OrderCommandEndpoints> logger,
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
        ILogger<OrderCommandEndpoints> logger,
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
        ILogger<OrderCommandEndpoints> logger,
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
