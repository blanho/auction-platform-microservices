using BuildingBlocks.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;

namespace Payment.Infrastructure.Persistence;

public sealed class WalletPaymentExecutor(PaymentDbContext db) : IWalletPaymentExecutor
{
    public Task<Result<WalletTransactionDto>> ExecuteAsync(
        Guid referenceId,
        Func<CancellationToken, Task<Result<WalletTransactionDto>>> payment,
        CancellationToken cancellationToken)
    {
        return db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var committed = false;
            try
            {
                var key = $"wallet-payment:{referenceId:D}";
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))", cancellationToken);
                var result = await payment(cancellationToken);
                if (result.IsSuccess)
                {
                    await transaction.CommitAsync(cancellationToken);
                    committed = true;
                }
                return result;
            }
            finally
            {
                if (!committed)
                {
                    db.ChangeTracker.Clear();
                }
            }
        });
    }
}
