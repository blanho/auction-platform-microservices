using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Jobs.Domain.ValueObjects;

public sealed class ExecutionContext : ValueObject
{
    public string MachineName { get; }
    public string? OperationName { get; }
    public int? BatchNumber { get; }
    public int? BatchSize { get; }
    public string? AdditionalData { get; }

    public ExecutionContext(
        string machineName,
        string? operationName = null,
        int? batchNumber = null,
        int? batchSize = null,
        string? additionalData = null)
    {
        if (string.IsNullOrWhiteSpace(machineName))
            throw new DomainInvariantException("Machine name is required for execution context.");

        MachineName = machineName;
        OperationName = operationName;
        BatchNumber = batchNumber;
        BatchSize = batchSize;
        AdditionalData = additionalData;
    }

    public static ExecutionContext ForBatch(string machineName, string operationName, int batchNumber, int batchSize) =>
        new(machineName, operationName, batchNumber, batchSize);

    public static ExecutionContext ForOperation(string machineName, string operationName) =>
        new(machineName, operationName);

    public static ExecutionContext ForMachine(string machineName) =>
        new(machineName);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MachineName;
        yield return OperationName ?? string.Empty;
        yield return BatchNumber ?? 0;
        yield return BatchSize ?? 0;
        yield return AdditionalData ?? string.Empty;
    }
}
