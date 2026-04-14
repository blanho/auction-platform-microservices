using Carter;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Features.Orders.CancelOrder;
using Payment.Application.Features.Orders.CreateOrder;
using Payment.Application.Features.Orders.MarkDelivered;
using Payment.Application.Features.Orders.ProcessPayment;
using Payment.Application.Features.Orders.ShipOrder;
using Payment.Application.Features.Orders.UpdateOrderStatus;
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
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Create));

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateOrder")
            .WithSummary("Update an order")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Cancel));

        group.MapPost("/{id:guid}/payment", ProcessPayment)
            .WithName("ProcessOrderPayment")
            .WithSummary("Process payment for an order")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Payments.Process));

        group.MapPost("/{id:guid}/ship", ShipOrder)
            .WithName("ShipOrder")
            .WithSummary("Mark order as shipped")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Cancel));

        group.MapPost("/{id:guid}/cancel", CancelOrder)
            .WithName("CancelOrder")
            .WithSummary("Cancel an order")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Cancel));

        group.MapPost("/{id:guid}/deliver", MarkDelivered)
            .WithName("MarkOrderDelivered")
            .WithSummary("Mark order as delivered")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Orders.Cancel));
    }

    private static async Task<IResult> Create(
        CreateOrderDto dto,
        IValidator<CreateOrderDto> validator,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Order.ValidationFailed", errors)));
        }

        var command = new CreateOrderCommand(
            dto.AuctionId,
            dto.BuyerId!.Value,
            dto.BuyerUsername!,
            dto.SellerId!.Value,
            dto.SellerUsername!,
            dto.ItemTitle!,
            dto.WinningBid!.Value,
            dto.ShippingCost,
            dto.PlatformFee,
            dto.ShippingAddress,
            dto.BuyerNotes
        );

        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Order created: {OrderId} for auction {AuctionId}", order.Id, order.AuctionId);
            return Results.Created($"/api/v1/orders/{order.Id}", order);
        });
    }

    private static async Task<IResult> Update(
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

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Order updated: {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }

    private static async Task<IResult> ProcessPayment(
        Guid id,
        ProcessPaymentDto dto,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand(id, dto.PaymentMethod, dto.ExternalTransactionId);
        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Payment processed for order: {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }

    private static async Task<IResult> ShipOrder(
        Guid id,
        UpdateShippingDto dto,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new ShipOrderCommand(id, dto.TrackingNumber, dto.ShippingCarrier, dto.SellerNotes);
        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Order shipped: {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }

    private static async Task<IResult> CancelOrder(
        Guid id,
        CancelOrderDto dto,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(id, dto.Reason);
        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Order cancelled: {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }

    private static async Task<IResult> MarkDelivered(
        Guid id,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new MarkDeliveredCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Order marked as delivered: {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }
}
