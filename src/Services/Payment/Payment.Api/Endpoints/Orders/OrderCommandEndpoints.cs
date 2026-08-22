using Carter;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Helpers;
using MediatR;
using Payment.Application.Features.Orders.MarkDelivered;
using Payment.Application.Features.Orders.PrepareCheckout;
using Payment.Application.Features.Orders.ShipOrder;
using Payment.Application.DTOs;

namespace Payment.Api.Endpoints.Orders;

public class OrderCommandEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        group.MapPut("/{id:guid}/checkout", PrepareCheckout)
            .WithName("PrepareOrderCheckout")
            .WithSummary("Save buyer-provided checkout details for an existing order")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/ship", ShipOrder)
            .WithName("ShipOrder")
            .WithSummary("Mark order as shipped")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/deliver", MarkDelivered)
            .WithName("MarkOrderDelivered")
            .WithSummary("Mark order as delivered")
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> PrepareCheckout(
        Guid id,
        PrepareCheckoutDto dto,
        HttpContext httpContext,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new PrepareCheckoutCommand(
            id,
            UserHelper.GetRequiredUserId(httpContext.User),
            dto.ShippingAddress,
            dto.BuyerNotes);

        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Checkout details saved for order {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }

    private static async Task<IResult> ShipOrder(
        Guid id,
        UpdateShippingDto dto,
        HttpContext httpContext,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var canManageAll = httpContext.User.IsAdmin() ||
                           httpContext.User.HasPermission(Permissions.Orders.Ship);
        var command = new ShipOrderCommand(
            id,
            userId,
            canManageAll,
            dto.TrackingNumber,
            dto.ShippingCarrier,
            dto.SellerNotes);
        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Order shipped: {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }

    private static async Task<IResult> MarkDelivered(
        Guid id,
        HttpContext httpContext,
        IMediator mediator,
        ILogger<OrderCommandEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var canManageAll = httpContext.User.IsAdmin() ||
                           httpContext.User.HasPermission(Permissions.Orders.Deliver);
        var command = new MarkDeliveredCommand(id, userId, canManageAll);
        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(order =>
        {
            logger.LogInformation("Order marked as delivered: {OrderId}", order.Id);
            return Results.Ok(order);
        });
    }
}
