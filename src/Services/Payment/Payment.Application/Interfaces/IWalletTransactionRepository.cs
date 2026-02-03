using BuildingBlocks.Application.Abstractions;
using Payment.Application.Filtering;
using Payment.Domain.Entities;

namespace Payment.Application.Interfaces;

public interface IWalletTransactionRepository
{
    Task<WalletTransaction> GetByIdAsync(Guid id);
    Task<WalletTransaction?> GetByReferenceIdAsync(string referenceId, TransactionType type, CancellationToken cancellationToken = default);
    Task<PaginatedResult<WalletTransaction>> GetByUsernameAsync(WalletTransactionQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<WalletTransaction> AddAsync(WalletTransaction transaction);
    Task<WalletTransaction> UpdateAsync(WalletTransaction transaction);
    Task<int> GetCountByUsernameAsync(string username);
    Task<decimal> GetTotalByUsernameAndTypeAsync(string username, TransactionType type);
}
