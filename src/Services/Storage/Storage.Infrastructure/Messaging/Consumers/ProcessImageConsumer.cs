using System.Security.Cryptography;
using JobService.Contracts.Commands;
using Storage.Domain.Enums;
using Storage.Infrastructure.Persistence;
using Storage.Infrastructure.Processing;
using StorageService.Contracts.Commands;

namespace Storage.Infrastructure.Messaging.Consumers;

public sealed class ProcessImageConsumer(StorageDbContext db, StorageJobFiles files,
    ImageProcessor processor, IUnitOfWork unitOfWork) : IConsumer<ProcessImageCommand>
{
    public async Task Consume(ConsumeContext<ProcessImageCommand> context)
    {
        var message = context.Message;
        if (!message.ParentJobId.HasValue || !message.ParentJobItemId.HasValue)
            throw new StorageJobValidationException("A parent job and item are required.");
        await using var transaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(context.CancellationToken) : null;
        string? error = null;
        try
        {
            var file = await files.LockOwnedFileAsync(message.FileId, message.RequestedBy, context.CancellationToken);
            if (file.Status != FileStatus.Ready)
                throw new StorageJobValidationException("Only confirmed uploads can be processed.");
            if (!ImageProcessor.Supports(file.ContentType))
                throw new StorageJobValidationException("Only JPEG, PNG, GIF, and WebP images are supported.");
            var bytes = await files.ReadAsync(file, context.CancellationToken);
            var checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            if (file.Checksum != checksum || !file.Metadata.TryGetValue(StorageDefaults.Images.VersionKey, out var version) ||
                version != StorageDefaults.Images.ProcessingVersion)
            {
                var processed = await processor.ProcessAsync(bytes, file.ContentType, context.CancellationToken);
                file.RecordProcessedImage(checksum, processed.Width, processed.Height, processed.ThumbnailDataUrl);
            }
        }
        catch (StorageJobValidationException ex) { error = ex.Message; }

        await context.Publish(new ReportJobItemResultCommand
        {
            JobId = message.ParentJobId.Value,
            JobItemId = message.ParentJobItemId.Value,
            Attempt = message.Attempt,
            IsSuccess = error is null,
            IsFinalFailure = error is not null,
            ErrorMessage = error
        }, context.CancellationToken);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        if (transaction is not null) await transaction.CommitAsync(context.CancellationToken);
    }
}
