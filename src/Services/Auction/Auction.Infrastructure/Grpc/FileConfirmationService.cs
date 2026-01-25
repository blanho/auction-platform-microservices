namespace Auctions.Infrastructure.Grpc;

public sealed class FileConfirmationService : IFileConfirmationService
{
    private readonly IFileStorageGrpcClient _fileStorageClient;

    public FileConfirmationService(IFileStorageGrpcClient fileStorageClient)
    {
        _fileStorageClient = fileStorageClient;
    }

    public async Task ConfirmFilesAsync(
        IEnumerable<Guid> fileIds, 
        string entityType, 
        string entityId, 
        CancellationToken cancellationToken = default)
    {
        var filesToConfirm = fileIds.Select(fileId => (fileId, entityType, entityId));
        await _fileStorageClient.ConfirmFilesBatchAsync(filesToConfirm, cancellationToken);
    }
}

