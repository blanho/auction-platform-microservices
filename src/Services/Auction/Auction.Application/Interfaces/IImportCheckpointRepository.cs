using Auctions.Application.Features.Auctions.ImportAuctions;

namespace Auctions.Application.Interfaces;

public interface IImportCheckpointRepository
{
    Task<ImportCheckpoint?> GetCheckpointAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    Task SaveCheckpointAsync(
        ImportCheckpoint checkpoint,
        CancellationToken cancellationToken = default);

    Task DeleteCheckpointAsync(
        string correlationId,
        CancellationToken cancellationToken = default);
}
