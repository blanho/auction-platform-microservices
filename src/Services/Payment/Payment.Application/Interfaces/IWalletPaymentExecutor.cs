namespace Payment.Application.Interfaces;

public interface IWalletPaymentExecutor
{
    Task<Result<WalletTransactionDto>> ExecuteAsync(
        Guid referenceId,
        Func<CancellationToken, Task<Result<WalletTransactionDto>>> payment,
        CancellationToken cancellationToken);
}
