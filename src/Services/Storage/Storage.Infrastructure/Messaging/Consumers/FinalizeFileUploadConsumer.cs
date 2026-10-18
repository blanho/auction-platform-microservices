using BuildingBlocks.Application.Abstractions.Storage;
using JobService.Contracts.Commands;
using Microsoft.Extensions.Options;
using Storage.Domain.Enums;
using Storage.Infrastructure.Persistence;
using Storage.Infrastructure.Processing;
using StorageService.Contracts.Commands;

namespace Storage.Infrastructure.Messaging.Consumers;

public sealed class FinalizeFileUploadConsumer(StorageDbContext db, StorageJobFiles files,
    IFileStorageService storage, IOptions<FileStorageSettings> settings, IUnitOfWork unitOfWork)
    : IConsumer<FinalizeFileUploadCommand>
{
    public async Task Consume(ConsumeContext<FinalizeFileUploadCommand> context)
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
            {
                if (file.Status != FileStatus.Pending || !file.UploadExpiresAt.HasValue || file.UploadExpiresAt <= DateTimeOffset.UtcNow)
                    throw new StorageJobValidationException("The upload reservation is no longer valid.");
                var validation = settings.Value.Validation;
                if (!validation.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase) ||
                    !validation.AllowedExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
                    throw new StorageJobValidationException("The reserved file type is not allowed.");
                await files.ReadAsync(file, context.CancellationToken);
                var url = await storage.GetUrlAsync(file.StoredFileName, context.CancellationToken);
                if (string.IsNullOrWhiteSpace(url))
                    throw new StorageJobValidationException("The uploaded file has no storage URL.");
                if (file.UploadExpiresAt <= DateTimeOffset.UtcNow)
                    throw new StorageJobValidationException("The upload reservation expired while validating the file.");
                file.ConfirmUpload(url);
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
