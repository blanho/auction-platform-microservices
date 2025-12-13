using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.Domain.Entities;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Services;

public class OrderService : IOrderService
{
    private readonly UtilityDbContext _context;
    private const decimal PlatformFeePercentage = 0.05m;

    public OrderService(UtilityDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, string buyerUsername, CancellationToken cancellationToken = default)
    {
        var platformFee = dto.WinningBid * PlatformFeePercentage;
        var totalAmount = dto.WinningBid + (dto.ShippingCost ?? 0);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            AuctionId = dto.AuctionId,
            BuyerUsername = buyerUsername,
            SellerUsername = dto.SellerUsername,
            ItemTitle = dto.ItemTitle,
            WinningBid = dto.WinningBid,
            ShippingCost = dto.ShippingCost,
            PlatformFee = platformFee,
            TotalAmount = totalAmount,
            Status = OrderStatus.PendingPayment,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([id], cancellationToken);
        return order != null ? MapToDto(order) : null;
    }

    public async Task<OrderDto?> GetOrderByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.AuctionId == auctionId, cancellationToken);
        return order != null ? MapToDto(order) : null;
    }

    public async Task<List<OrderDto>> GetBuyerOrdersAsync(string username, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.BuyerUsername == username)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    public async Task<List<OrderDto>> GetSellerOrdersAsync(string username, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.SellerUsername == username)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto dto, string username, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([id], cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        if (order.SellerUsername != username && order.BuyerUsername != username)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this order");
        }

        if (Enum.TryParse<OrderStatus>(dto.Status, true, out var status))
        {
            order.Status = status;
        }

        if (!string.IsNullOrEmpty(dto.TrackingNumber))
        {
            order.TrackingNumber = dto.TrackingNumber;
        }

        if (!string.IsNullOrEmpty(dto.ShippingCarrier))
        {
            order.ShippingCarrier = dto.ShippingCarrier;
        }

        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(string username, CancellationToken cancellationToken = default)
    {
        var orders = await _context.Orders
            .Where(o => o.SellerUsername == username || o.BuyerUsername == username)
            .ToListAsync(cancellationToken);

        return new OrderSummaryDto(
            TotalOrders: orders.Count,
            PendingPayment: orders.Count(o => o.Status == OrderStatus.PendingPayment),
            AwaitingShipment: orders.Count(o => o.Status == OrderStatus.PaymentReceived || o.Status == OrderStatus.Processing),
            Shipped: orders.Count(o => o.Status == OrderStatus.Shipped),
            Completed: orders.Count(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Delivered),
            TotalRevenue: orders.Where(o => o.SellerUsername == username && o.PaymentStatus == PaymentStatus.Completed).Sum(o => o.WinningBid)
        );
    }

    public async Task MarkAsPaidAsync(Guid id, string transactionId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([id], cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        order.PaymentStatus = PaymentStatus.Completed;
        order.PaymentTransactionId = transactionId;
        order.PaidAt = DateTimeOffset.UtcNow;
        order.Status = OrderStatus.PaymentReceived;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsShippedAsync(Guid id, string trackingNumber, string carrier, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([id], cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        order.TrackingNumber = trackingNumber;
        order.ShippingCarrier = carrier;
        order.ShippedAt = DateTimeOffset.UtcNow;
        order.Status = OrderStatus.Shipped;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsDeliveredAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FindAsync([id], cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        order.DeliveredAt = DateTimeOffset.UtcNow;
        order.Status = OrderStatus.Delivered;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.AuctionId,
            order.BuyerUsername,
            order.SellerUsername,
            order.ItemTitle,
            order.WinningBid,
            order.TotalAmount,
            order.ShippingCost,
            order.PlatformFee,
            order.Status.ToString(),
            order.PaymentStatus.ToString(),
            order.TrackingNumber,
            order.ShippingCarrier,
            order.PaidAt,
            order.ShippedAt,
            order.DeliveredAt,
            order.CreatedAt
        );
    }
}
