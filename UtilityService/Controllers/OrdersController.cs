using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByIdAsync(id, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        var username = User.Identity?.Name;
        if (order.BuyerUsername != username && order.SellerUsername != username && !User.IsInRole("admin"))
        {
            return Forbid();
        }

        return Ok(order);
    }

    [HttpGet("auction/{auctionId:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderByAuction(Guid auctionId, CancellationToken cancellationToken)
    {
        var order = await _orderService.GetOrderByAuctionIdAsync(auctionId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        var username = User.Identity?.Name;
        if (order.BuyerUsername != username && order.SellerUsername != username && !User.IsInRole("admin"))
        {
            return Forbid();
        }

        return Ok(order);
    }

    [HttpGet("purchases")]
    public async Task<ActionResult<List<OrderDto>>> GetMyPurchases(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var orders = await _orderService.GetBuyerOrdersAsync(username, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("sales")]
    public async Task<ActionResult<List<OrderDto>>> GetMySales(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var orders = await _orderService.GetSellerOrdersAsync(username, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<OrderSummaryDto>> GetOrderSummary(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        var summary = await _orderService.GetOrderSummaryAsync(username, cancellationToken);
        return Ok(summary);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized();
        }

        try
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, dto, username, cancellationToken);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> MarkAsShipped(Guid id, [FromBody] ShipOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.MarkAsShippedAsync(id, dto.TrackingNumber, dto.Carrier, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id:guid}/delivered")]
    public async Task<IActionResult> MarkAsDelivered(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _orderService.MarkAsDeliveredAsync(id, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public record ShipOrderDto(string TrackingNumber, string Carrier);
