using Common.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Common.Logging.Implementations;

public class LoggerAdapter<T> : ILoggerAdapter<T>
{
    private readonly ILogger<T> _logger;

    public LoggerAdapter(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
        => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args)
        => _logger.LogWarning(message, args);

    public void LogError(string message, params object[] args)
        => _logger.LogError(message, args);

    public void LogError(Exception exception, string message, params object[] args)
        => _logger.LogError(exception, message, args);

    public void LogDebug(string message, params object[] args)
        => _logger.LogDebug(message, args);

    public void LogTrace(string message, params object[] args)
        => _logger.LogTrace(message, args);
}
