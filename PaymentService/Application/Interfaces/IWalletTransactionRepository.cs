using Common.Core.Constants;
using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IWalletTransactionRepository
{
    Task<WalletTransaction> GetByIdAsync(Guid id);
    Task<IEnumerable<WalletTransaction>> GetByUsernameAsync(string username, int page = PaginationDefaults.DefaultPage, int pageSize = PaginationDefaults.DefaultPageSize);
    Task<WalletTransaction> AddAsync(WalletTransaction transaction);
    Task<WalletTransaction> UpdateAsync(WalletTransaction transaction);
    Task<int> GetCountByUsernameAsync(string username);
    Task<decimal> GetTotalByUsernameAndTypeAsync(string username, TransactionType type);
}
