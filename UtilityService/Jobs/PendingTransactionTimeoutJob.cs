using Common.Scheduling.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UtilityService.Interfaces;

namespace UtilityService.Jobs;

public class PendingTransactionTimeoutJob : BaseJob
{
    public const string JobId = "pending-transaction-timeout";
    public const string Description = "Times out stale pending wallet transactions";

    private static readonly TimeSpan TransactionTimeout = TimeSpan.FromHours(24);

    public PendingTransactionTimeoutJob(
        ILogger<PendingTransactionTimeoutJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var walletRepository = scopedProvider.GetRequiredService<IWalletRepository>();
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();

        var stalePendingTransactions = await walletRepository
            .GetTimedOutPendingTransactionsAsync(TransactionTimeout, cancellationToken);

        if (stalePendingTransactions.Count == 0)
        {
            return;
        }

        Logger.LogInformation(
            "Found {Count} stale pending transactions to timeout",
            stalePendingTransactions.Count);

        var timedOutCount = 0;

        foreach (var transaction in stalePendingTransactions)
        {
            try
            {
                transaction.Status = Domain.Entities.TransactionStatus.Cancelled;
                transaction.ProcessedAt = DateTimeOffset.UtcNow;
                transaction.Description = $"{transaction.Description} [Timed out after {TransactionTimeout.TotalHours}h]";

                timedOutCount++;
                Logger.LogDebug(
                    "Timed out transaction {TransactionId} for user {Username}",
                    transaction.Id, transaction.Username);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to timeout transaction {TransactionId}", transaction.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Timed out {Count} pending transactions", timedOutCount);
    }
}
