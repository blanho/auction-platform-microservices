using PaymentService.Domain.Entities;

namespace PaymentService.Application.Interfaces;

public interface IWalletTransactionRepository
{
    Task<WalletTransaction> GetByIdAsync(Guid id);
    Task<IEnumerable<WalletTransaction>> GetByUsernameAsync(string username, int page = 1, int pageSize = 10);
    Task<WalletTransaction> AddAsync(WalletTransaction transaction);
    Task<WalletTransaction> UpdateAsync(WalletTransaction transaction);
    Task<int> GetCountByUsernameAsync(string username);
    Task<decimal> GetTotalByUsernameAndTypeAsync(string username, TransactionType type);
}
