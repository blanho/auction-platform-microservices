using AutoMapper;
using Payment.Application.DTOs;
using Payment.Domain.Entities;

namespace Payment.Application.Extensions.Mappings;

public static class WalletTransactionMappingExtensions
{
    public static WalletTransactionDto ToDto(this WalletTransaction transaction, IMapper mapper)
    {
        return mapper.Map<WalletTransactionDto>(transaction);
    }

    public static List<WalletTransactionDto> ToDtoList(this IEnumerable<WalletTransaction> transactions, IMapper mapper)
    {
        return transactions.Select(t => t.ToDto(mapper)).ToList();
    }
}
