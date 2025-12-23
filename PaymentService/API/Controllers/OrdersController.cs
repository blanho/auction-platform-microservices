using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpGet("auction/{auctionId:guid}")]
    public async Task<ActionResult<OrderDto>> GetByAuctionId(Guid auctionId, CancellationToken cancellationToken)
    {
        var query = new GetOrderByAuctionIdQuery(auctionId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        if (result.Value == null)
            return NotFound();

        return Ok(result.Value);
    }

    [HttpGet("buyer/{username}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByBuyer(
        string username, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersByBuyerQuery(username, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Append("X-Page", result.Value.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.Value.PageSize.ToString());

        return Ok(result.Value.Items);
    }

    [HttpGet("seller/{username}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetBySeller(
        string username, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrdersBySellerQuery(username, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
        Response.Headers.Append("X-Page", result.Value.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.Value.PageSize.ToString());

        return Ok(result.Value.Items);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
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

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        _logger.LogInformation("Order created: {OrderId} for auction {AuctionId}", result.Value.Id, result.Value.AuctionId);

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(Guid id, [FromBody] UpdateOrderDto dto, CancellationToken cancellationToken)
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

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        _logger.LogInformation("Order updated: {OrderId}", result.Value.Id);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/payment")]
    public async Task<ActionResult<OrderDto>> ProcessPayment(Guid id, [FromBody] ProcessPaymentDto dto, CancellationToken cancellationToken)
    {
        var command = new ProcessPaymentCommand(id, dto.PaymentMethod, dto.ExternalTransactionId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        _logger.LogInformation("Payment processed for order: {OrderId}", result.Value.Id);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/ship")]
    public async Task<ActionResult<OrderDto>> ShipOrder(Guid id, [FromBody] UpdateShippingDto dto, CancellationToken cancellationToken)
    {
        var command = new ShipOrderCommand(id, dto.TrackingNumber, dto.ShippingCarrier, dto.SellerNotes);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        _logger.LogInformation("Order shipped: {OrderId}", result.Value.Id);

        return Ok(result.Value);
    }
}
