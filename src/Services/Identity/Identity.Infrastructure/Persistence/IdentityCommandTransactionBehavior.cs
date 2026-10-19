using BuildingBlocks.Application.CQRS.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;

namespace Identity.Infrastructure.Persistence;

public sealed class IdentityCommandTransactionBehavior<TRequest, TResponse>(ApplicationDbContext db)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly bool IsCommand = typeof(ICommand).IsAssignableFrom(typeof(TRequest)) ||
        typeof(TRequest).GetInterfaces().Any(type =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICommand<>));

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand || db.Database.CurrentTransaction != null)
            return await next();

        return await new CommandExecutionStrategy(db).ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var response = await next();
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return response;
            }
            catch
            {
                db.ChangeTracker.Clear();
                throw;
            }
        });
    }

    private sealed class CommandExecutionStrategy(ApplicationDbContext context)
        : ExecutionStrategy(context, maxRetryCount: 0, maxRetryDelay: TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception) => false;
    }
}
