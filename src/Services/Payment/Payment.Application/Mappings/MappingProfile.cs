using AutoMapper;
using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDto>();

        CreateMap<Wallet, WalletDto>();

        CreateMap<WalletTransaction, WalletTransactionDto>();
        CreateMap<CreateWalletTransactionDto, WalletTransaction>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => TransactionStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTimeOffset.UtcNow));
    }
}
