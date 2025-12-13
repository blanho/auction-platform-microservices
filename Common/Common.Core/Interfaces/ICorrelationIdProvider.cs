namespace Common.Core.Interfaces;

public interface ICorrelationIdProvider
{
    string Get();
    void Set(string correlationId);
}
