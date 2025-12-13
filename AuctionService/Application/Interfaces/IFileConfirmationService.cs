namespace AuctionService.Application.Interfaces;

public interface IFileConfirmationService
{
    Task ConfirmFilesAsync(
        IEnumerable<Guid> fileIds, 
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken = default);
}
