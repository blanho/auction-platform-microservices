using AutoMapper;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IWalletService _walletService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IWalletService walletService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _walletService = walletService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<OrderDto?> GetOrderByAuctionIdAsync(Guid auctionId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByAuctionIdAsync(auctionId);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByBuyerAsync(string buyerUsername, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetByBuyerUsernameAsync(buyerUsername, page, pageSize);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersBySellerAsync(string sellerUsername, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetBySellerUsernameAsync(sellerUsername, page, pageSize);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> CreateOrderFromAuctionAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var existingOrder = await _orderRepository.GetByAuctionIdAsync(dto.AuctionId);
        if (existingOrder != null)
            throw new InvalidOperationException("Order already exists for this auction");

        var totalAmount = dto.WinningBid + (dto.ShippingCost ?? 0) + (dto.PlatformFee ?? 0);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            AuctionId = dto.AuctionId,
            BuyerId = dto.BuyerId,
            BuyerUsername = dto.BuyerUsername,
            SellerId = dto.SellerId,
            SellerUsername = dto.SellerUsername,
            ItemTitle = dto.ItemTitle,
            WinningBid = dto.WinningBid,
            TotalAmount = totalAmount,
            ShippingCost = dto.ShippingCost,
            PlatformFee = dto.PlatformFee,
            Status = OrderStatus.PendingPayment,
            PaymentStatus = PaymentStatus.Pending
        };

        order.RaiseCreatedEvent();

        var created = await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created for auction {AuctionId}", order.Id, dto.AuctionId);

        return _mapper.Map<OrderDto>(created);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string? notes, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException("Order not found");

        var oldStatus = order.Status;
        order.ChangeStatus(newStatus);
        order.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrEmpty(notes))
        {
            order.SellerNotes = notes;
        }

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} status changed from {OldStatus} to {NewStatus}", 
            orderId, oldStatus, newStatus);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> UpdateShippingAsync(Guid orderId, UpdateShippingDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException("Order not found");

        order.MarkAsShipped(dto.TrackingNumber, dto.ShippingCarrier);
        order.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrEmpty(dto.SellerNotes))
        {
            order.SellerNotes = dto.SellerNotes;
        }

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} shipped with tracking {TrackingNumber}", 
            orderId, dto.TrackingNumber);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException("Order not found");

        if (order.PaymentStatus == PaymentStatus.Completed)
            throw new InvalidOperationException("Order is already paid");

        order.CompletePayment(dto.ExternalTransactionId);
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment processed for order {OrderId}", orderId);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> ConfirmDeliveryAsync(Guid orderId, string username, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException("Order not found");

        if (order.BuyerUsername != username)
            throw new UnauthorizedAccessException("Only the buyer can confirm delivery");

        if (order.Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Order must be shipped before confirming delivery");

        order.MarkAsDelivered();
        order.UpdatedAt = DateTimeOffset.UtcNow;

        await _orderRepository.UpdateAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Delivery confirmed for order {OrderId} by {Username}", orderId, username);

        return _mapper.Map<OrderDto>(order);
    }
}
