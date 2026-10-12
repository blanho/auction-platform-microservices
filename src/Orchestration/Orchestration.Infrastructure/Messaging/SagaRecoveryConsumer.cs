using MassTransit;
using Microsoft.EntityFrameworkCore;
using Orchestration.Infrastructure.Persistence;
using Orchestration.Sagas;
using OrchestrationService.Contracts.Events;

namespace Orchestration.Infrastructure.Messaging;

public class SagaRecoveryConsumer(OrchestrationDbContext dbContext) :
    IConsumer<SagaRecoveryRequired>,
    IConsumer<BuyNowSagaCompleted>,
    IConsumer<AuctionCompletionSagaCompleted>,
    IConsumer<AuctionCompletionNotificationsSent>
{
    public async Task Consume(ConsumeContext<SagaRecoveryRequired> context)
    {
        var message = context.Message;
        var recoveryCase = await GetOrCreateCaseAsync(
            message.Workflow, message.CorrelationId, message.AuctionId, message.Step,
            context.CancellationToken);

        if (!recoveryCase.ResolvedAt.HasValue)
        {
            recoveryCase.Reason = message.Reason;
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }

    public Task Consume(ConsumeContext<BuyNowSagaCompleted> context) =>
        ResolveStepsAsync(
            SagaConstants.Workflows.BuyNow,
            context.Message.CorrelationId,
            context.Message.AuctionId,
            [SagaConstants.RecoverySteps.ReleaseReservation,
             SagaConstants.RecoverySteps.CancelOrder,
             SagaConstants.RecoverySteps.ConfirmOrder],
            context.CancellationToken);

    public Task Consume(ConsumeContext<AuctionCompletionSagaCompleted> context) =>
        ResolveStepsAsync(
            SagaConstants.Workflows.AuctionCompletion,
            context.Message.CorrelationId,
            context.Message.AuctionId,
            [SagaConstants.RecoverySteps.CreateOrder],
            context.CancellationToken);

    public Task Consume(ConsumeContext<AuctionCompletionNotificationsSent> context) =>
        ResolveStepsAsync(
            SagaConstants.Workflows.AuctionCompletion,
            context.Message.CorrelationId,
            context.Message.AuctionId,
            [SagaConstants.RecoverySteps.Notifications],
            context.CancellationToken);

    private async Task<SagaRecoveryCase> GetOrCreateCaseAsync(
        string workflow,
        Guid correlationId,
        Guid auctionId,
        string step,
        CancellationToken cancellationToken)
    {
        // The receive outbox transaction holds this lock through the case update.
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({workflow + correlationId}, 0))",
            cancellationToken);

        var recoveryCase = await dbContext.RecoveryCases.FindAsync(
            [workflow, correlationId, step], cancellationToken);
        if (recoveryCase is not null)
        {
            return recoveryCase;
        }

        recoveryCase = new SagaRecoveryCase
        {
            Workflow = workflow,
            CorrelationId = correlationId,
            AuctionId = auctionId,
            Step = step
        };
        dbContext.Add(recoveryCase);
        return recoveryCase;
    }

    private async Task ResolveStepsAsync(
        string workflow,
        Guid correlationId,
        Guid auctionId,
        string[] steps,
        CancellationToken cancellationToken)
    {
        foreach (var step in steps)
        {
            // Keep resolved cases even when completion arrives before the failure event.
            var recoveryCase = await GetOrCreateCaseAsync(
                workflow, correlationId, auctionId, step, cancellationToken);
            recoveryCase.ResolvedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
