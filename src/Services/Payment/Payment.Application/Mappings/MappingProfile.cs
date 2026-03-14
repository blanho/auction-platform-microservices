using AutoMapper;
using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => 
                src.WinningBid + (src.ShippingCost ?? 0) + (src.PlatformFee ?? 0)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => OrderStatus.PaymentPending))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(_ => PaymentStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));

        CreateMap<Wallet, WalletDto>();
        
        CreateMap<WalletTransaction, WalletTransactionDto>();
        CreateMap<CreateWalletTransactionDto, WalletTransaction>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TransactionStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));
    }
}
