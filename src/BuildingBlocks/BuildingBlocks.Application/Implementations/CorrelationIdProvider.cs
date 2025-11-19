using BuildingBlocks.Application.Abstractions.Providers;

namespace BuildingBlocks.Application.Implementations;

public class CorrelationIdProvider : ICorrelationIdProvider
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public string Get()
    {
        return _correlationId.Value ?? Guid.NewGuid().ToString();
    }

    public void Set(string correlationId)
    {
        _correlationId.Value = correlationId;
    }
}
