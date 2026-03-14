using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IUserContext? _userContext;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        IUserContext? userContext = null)
    {
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var correlationId = _userContext?.CorrelationId ?? Guid.NewGuid().ToString();

        _logger.LogInformation(
            "[START] {RequestName} CorrelationId={CorrelationId} UserId={UserId}",
            requestName,
            correlationId,
            _userContext?.UserIdOrDefault);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "[END] {RequestName} CorrelationId={CorrelationId} completed in {ElapsedMilliseconds}ms",
                requestName,
                correlationId,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                "[FAILED] {RequestName} CorrelationId={CorrelationId} failed after {ElapsedMilliseconds}ms",
                requestName,
                correlationId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
