namespace BuildingBlocks.Application.Abstractions.Logging;

[System.Obsolete("Use ILogger<T> from Microsoft.Extensions.Logging directly instead. This wrapper adds no value.")]
public interface IAppLogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
    void LogDebug(string message, params object[] args);
}
