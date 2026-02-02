using AutoMapper;
using Payment.Application.DTOs;
using Payment.Application.Errors;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Orders.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<CreateOrderCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating order for auction {AuctionId}", request.AuctionId);

        var existingOrder = await _repository.GetByAuctionIdAsync(request.AuctionId);
        if (existingOrder != null)
        {
            return Result.Failure<OrderDto>(PaymentErrors.Order.AlreadyExistsForAuction(request.AuctionId));
        }

        var order = Order.Create(
            auctionId: request.AuctionId,
            buyerId: request.BuyerId,
            buyerUsername: request.BuyerUsername,
            sellerId: request.SellerId,
            sellerUsername: request.SellerUsername,
            itemTitle: request.ItemTitle,
            winningBid: request.WinningBid);

        if (request.ShippingCost.HasValue)
            order.SetShippingCost(request.ShippingCost.Value);

        if (!string.IsNullOrWhiteSpace(request.ShippingAddress))
            order.SetShippingAddress(request.ShippingAddress);

        if (!string.IsNullOrWhiteSpace(request.BuyerNotes))
            order.AddBuyerNotes(request.BuyerNotes);

        order.RaiseCreatedEvent();

        var createdOrder = await _repository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Created order {OrderId} for auction {AuctionId}", createdOrder.Id, request.AuctionId);

        return _mapper.Map<OrderDto>(createdOrder);
    }
}
