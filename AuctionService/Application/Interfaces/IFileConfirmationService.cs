namespace AuctionService.Application.Interfaces;

/// <summary>
/// Service for confirming temporary files
/// </summary>
public interface IFileConfirmationService
{
    Task ConfirmFilesAsync(
        IEnumerable<Guid> fileIds, 
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken = default);
}
