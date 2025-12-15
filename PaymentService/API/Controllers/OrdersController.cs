using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderRepository orderRepository,
        IMapper mapper,
        ILogger<OrdersController> logger)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(_mapper.Map<OrderDto>(order));
    }

    [HttpGet("auction/{auctionId:guid}")]
    public async Task<ActionResult<OrderDto>> GetByAuctionId(Guid auctionId)
    {
        var order = await _orderRepository.GetByAuctionIdAsync(auctionId);
        if (order == null)
            return NotFound();

        return Ok(_mapper.Map<OrderDto>(order));
    }

    [HttpGet("buyer/{username}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByBuyer(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var orders = await _orderRepository.GetByBuyerUsernameAsync(username, page, pageSize);
        var totalCount = await _orderRepository.GetCountByBuyerUsernameAsync(username);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
    }

    [HttpGet("seller/{username}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetBySeller(string username, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var orders = await _orderRepository.GetBySellerUsernameAsync(username, page, pageSize);
        var totalCount = await _orderRepository.GetCountBySellerUsernameAsync(username);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());

        return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
    {
        var order = _mapper.Map<Order>(dto);
        var created = await _orderRepository.AddAsync(order);

        _logger.LogInformation("Order created: {OrderId} for auction {AuctionId}", created.Id, created.AuctionId);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, _mapper.Map<OrderDto>(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(Guid id, [FromBody] UpdateOrderDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        if (dto.Status.HasValue)
            order.Status = dto.Status.Value;
        if (dto.PaymentStatus.HasValue)
            order.PaymentStatus = dto.PaymentStatus.Value;
        if (!string.IsNullOrEmpty(dto.PaymentTransactionId))
            order.PaymentTransactionId = dto.PaymentTransactionId;
        if (!string.IsNullOrEmpty(dto.ShippingAddress))
            order.ShippingAddress = dto.ShippingAddress;
        if (!string.IsNullOrEmpty(dto.TrackingNumber))
            order.TrackingNumber = dto.TrackingNumber;
        if (!string.IsNullOrEmpty(dto.ShippingCarrier))
            order.ShippingCarrier = dto.ShippingCarrier;
        if (!string.IsNullOrEmpty(dto.BuyerNotes))
            order.BuyerNotes = dto.BuyerNotes;
        if (!string.IsNullOrEmpty(dto.SellerNotes))
            order.SellerNotes = dto.SellerNotes;

        if (dto.PaymentStatus == PaymentStatus.Completed && order.PaidAt == null)
            order.PaidAt = DateTimeOffset.UtcNow;
        if (dto.Status == OrderStatus.Shipped && order.ShippedAt == null)
            order.ShippedAt = DateTimeOffset.UtcNow;
        if (dto.Status == OrderStatus.Delivered && order.DeliveredAt == null)
            order.DeliveredAt = DateTimeOffset.UtcNow;

        var updated = await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order updated: {OrderId}", updated.Id);

        return Ok(_mapper.Map<OrderDto>(updated));
    }
}
