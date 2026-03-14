using AutoMapper;
using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Extensions.Mappings;

public static class WalletMappingExtensions
{
    public static WalletDto ToDto(this Wallet wallet, IMapper mapper)
    {
        return mapper.Map<WalletDto>(wallet);
    }

    public static List<WalletDto> ToDtoList(this IEnumerable<Wallet> wallets, IMapper mapper)
    {
        return wallets.Select(w => w.ToDto(mapper)).ToList();
    }
}
