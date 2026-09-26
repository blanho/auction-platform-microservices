namespace Payment.Application.Interfaces;

public interface IReportStorageClient
{
    Task<string> StoreAsync(OrderReportResult report, Guid ownerId, CancellationToken cancellationToken);
}
