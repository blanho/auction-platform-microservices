using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;
using Common.Repository.Interfaces;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<CreateOrderCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository repository,
        IMapper mapper,
        IAppLogger<CreateOrderCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for auction {AuctionId}", request.AuctionId);

        var existingOrder = await _repository.GetByAuctionIdAsync(request.AuctionId);
        if (existingOrder != null)
        {
            return Result.Failure<OrderDto>(Error.Create("Order.AlreadyExists", $"Order for auction {request.AuctionId} already exists"));
        }

        var totalAmount = request.WinningBid + (request.ShippingCost ?? 0) + (request.PlatformFee ?? 0);

        var order = new Order
        {
            AuctionId = request.AuctionId,
            BuyerId = request.BuyerId,
            BuyerUsername = request.BuyerUsername,
            SellerId = request.SellerId,
            SellerUsername = request.SellerUsername,
            ItemTitle = request.ItemTitle,
            WinningBid = request.WinningBid,
            TotalAmount = totalAmount,
            ShippingCost = request.ShippingCost,
            PlatformFee = request.PlatformFee,
            ShippingAddress = request.ShippingAddress,
            BuyerNotes = request.BuyerNotes,
            Status = OrderStatus.PendingPayment,
            PaymentStatus = PaymentStatus.Pending
        };

        order.RaiseCreatedEvent();

        var createdOrder = await _repository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created order {OrderId} for auction {AuctionId}", createdOrder.Id, request.AuctionId);

        return _mapper.Map<OrderDto>(createdOrder);
    }
}
