using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;
using Storage.Domain.Entities;
using Storage.Infrastructure.Persistence;
using StorageService.Contracts.Reports;

namespace Storage.Infrastructure.Processing;

public sealed class StorageJobFiles(
    StorageDbContext db,
    IFileStorageService storage,
    IOptions<FileStorageSettings> settings)
{
    public async Task<StoredFile> LockOwnedFileAsync(Guid fileId, Guid requestedBy, CancellationToken ct)
    {
        if (fileId == Guid.Empty || requestedBy == Guid.Empty)
            throw new StorageJobValidationException("FileId and requester are required.");
        var file = await db.StoredFiles.FromSqlInterpolated(
            $"SELECT *, xmin FROM \"StoredFiles\" WHERE \"Id\" = {fileId} FOR UPDATE").SingleOrDefaultAsync(ct);
        if (file is null || file.OwnerId != requestedBy || file.IsDeleted ||
            ReportStorageContract.IsPrivatePath(file.SubFolder) || ReportStorageContract.IsPrivatePath(file.StoredFileName))
            throw new StorageJobValidationException("File not found or not owned by the requester.");
        if (file.Provider != StorageDefaults.Providers.Resolve(settings.Value.Provider))
            throw new StorageJobValidationException("The file's storage provider is not configured.");
        if (file.FileSize <= 0 || file.FileSize > settings.Value.Validation.MaxFileSizeBytes)
            throw new StorageJobValidationException("The file exceeds the configured size limit.");
        return file;
    }

    public async Task<byte[]> ReadAsync(StoredFile file, CancellationToken ct)
    {
        var download = await storage.DownloadAsync(file.StoredFileName, ct)
            ?? throw new StorageJobValidationException("The uploaded file is missing from storage.");
        await using var input = download.Content;
        if (download.FileSize != file.FileSize)
            throw new StorageJobValidationException("The uploaded file size does not match its reservation.");
        await using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int count;
        while ((count = await input.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + count > file.FileSize)
                throw new StorageJobValidationException("The uploaded file exceeds its declared size.");
            await buffer.WriteAsync(chunk.AsMemory(0, count), ct);
        }
        if (buffer.Length != file.FileSize)
            throw new StorageJobValidationException("The uploaded file is incomplete.");
        return buffer.ToArray();
    }
}
