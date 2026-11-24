using System.Diagnostics;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMilliseconds = 500;

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
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestName"] = requestName,
            ["CorrelationId"] = correlationId
        });

        _logger.LogInformation(
            "[START] {RequestName} CorrelationId={CorrelationId} UserId={UserId}",
            requestName,
            correlationId,
            _userContext?.UserIdOrDefault);

        var startedAt = Stopwatch.GetTimestamp();
        var response = await next();
        var elapsedMilliseconds = Stopwatch.GetElapsedTime(startedAt).Ticks / TimeSpan.TicksPerMillisecond;

        if (elapsedMilliseconds > SlowRequestThresholdMilliseconds)
            _logger.LogWarning(
                "[SLOW] {RequestName} CorrelationId={CorrelationId} took {ElapsedMilliseconds}ms",
                requestName,
                correlationId,
                elapsedMilliseconds);

        _logger.LogInformation(
            "[END] {RequestName} CorrelationId={CorrelationId} completed in {ElapsedMilliseconds}ms",
            requestName,
            correlationId,
            elapsedMilliseconds);

        return response;
    }
}
