using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces;

public interface IWalletService
{
    Task<WalletDto?> GetWalletAsync(string username, CancellationToken cancellationToken = default);
    Task<WalletDto> CreateWalletAsync(Guid userId, string username, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> DepositAsync(string username, decimal amount, string? paymentMethod, string? description, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> WithdrawAsync(string username, decimal amount, string? paymentMethod, string? description, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> HoldFundsAsync(string username, decimal amount, Guid referenceId, string referenceType, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> ReleaseFundsAsync(string username, decimal amount, Guid referenceId, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto> ProcessPaymentAsync(string username, decimal amount, Guid referenceId, string description, CancellationToken cancellationToken = default);
    Task<(IEnumerable<WalletTransactionDto> Transactions, int TotalCount)> GetTransactionsAsync(string username, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<WalletTransactionDto?> GetTransactionByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
}
