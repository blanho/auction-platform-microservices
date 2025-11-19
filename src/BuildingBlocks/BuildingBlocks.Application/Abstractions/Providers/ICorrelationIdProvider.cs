namespace BuildingBlocks.Application.Abstractions.Providers;

public interface ICorrelationIdProvider
{
    string Get();
    void Set(string correlationId);
}
