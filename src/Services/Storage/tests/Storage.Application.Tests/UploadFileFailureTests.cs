using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Storage.Application.Features.Files.UploadFile;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using TestSupport;
using Xunit;

namespace Storage.Application.Tests;

public class UploadFileFailureTests
{
    [Theory]
    [InlineData("committed", false)]
    [InlineData("unknown", false)]
    [InlineData("missing", true)]
    [InlineData("different-blob", true)]
    public async Task ReportUploadFailure_OnlyDeletesBlobWhenItIsKnownToBeUncommitted(
        string persistenceOutcome, bool shouldDeleteBlob)
    {
        var ownerId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var databaseError = new IOException("commit acknowledgement lost");
        var lookups = 0;
        var deletes = 0;
        var repository = TestProxy.Create<IStoredFileRepository>((method, args) => method.Name switch
        {
            nameof(IStoredFileRepository.GetByReportRequestIdAsync) => Lookup(args!),
            nameof(IStoredFileRepository.AddAsync) => Task.CompletedTask,
            _ => throw new NotSupportedException(method.Name)
        });
        Task<StoredFile?> Lookup(object?[] args)
        {
            Assert.Equal(ownerId, args[0]);
            Assert.Equal(requestId, args[1]);
            if (++lookups == 1) return Task.FromResult<StoredFile?>(null);

            Assert.Equal(CancellationToken.None, args[2]);
            return persistenceOutcome switch
            {
                "unknown" => Task.FromException<StoredFile?>(new IOException("database unavailable")),
                "missing" => Task.FromResult<StoredFile?>(null),
                _ => Task.FromResult<StoredFile?>(StoredFile.Create(
                    "report.csv", persistenceOutcome == "committed" ? "stored.csv" : "other.csv",
                    "text/csv", 1, "/files/report.csv", null, ownerId, StorageProvider.Local))
            };
        }
        var storage = TestProxy.Create<IFileStorageService>((method, args) => method.Name switch
        {
            nameof(IFileStorageService.UploadAsync) => Task.FromResult(new FileUploadResult(
                "id", "report.csv", "stored.csv", "text/csv", 1, "/files/stored.csv", DateTimeOffset.UtcNow)),
            nameof(IFileStorageService.DeleteAsync) => Delete(args!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task<bool> Delete(object?[] args)
        {
            Assert.Equal("stored.csv", args[0]);
            Assert.Equal(CancellationToken.None, args[1]);
            deletes++;
            return Task.FromResult(true);
        }
        var unitOfWork = TestProxy.Create<IUnitOfWork>((method, _) => method.Name switch
        {
            nameof(IUnitOfWork.SaveChangesAsync) => Task.FromException<int>(databaseError),
            _ => throw new NotSupportedException(method.Name)
        });
        var handler = new UploadFileCommandHandler(storage, repository, unitOfWork,
            Options.Create(new FileStorageSettings()), NullLogger<UploadFileCommandHandler>.Instance, null!);

        var thrown = await Assert.ThrowsAsync<IOException>(() => handler.Handle(
            new UploadFileCommand(Stream.Null, "report.csv", "text/csv", 1,
                OwnerId: ownerId, ReportRequestId: requestId), CancellationToken.None));

        Assert.Same(databaseError, thrown);
        Assert.Equal(2, lookups);
        Assert.Equal(shouldDeleteBlob ? 1 : 0, deletes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_PreservesDatabaseErrorAndAttemptsBlobRollback(bool failOnAdd)
    {
        var databaseError = new InvalidOperationException("database failed");
        var rollbackAttempted = false;
        var storage = TestProxy.Create<IFileStorageService>((method, args) => method.Name switch
        {
            nameof(IFileStorageService.UploadAsync) => Task.FromResult(new FileUploadResult(
                "id", "report.csv", "stored.csv", "text/csv", 1, "/files/stored.csv", DateTimeOffset.UtcNow)),
            nameof(IFileStorageService.DeleteAsync) => Delete(args),
            _ => throw new NotSupportedException(method.Name)
        });

        Task<bool> Delete(object?[]? args)
        {
            Assert.Equal("stored.csv", args![0]);
            Assert.Equal(CancellationToken.None, args[1]);
            rollbackAttempted = true;
            return Task.FromException<bool>(new InvalidOperationException("storage failed"));
        }

        var repository = TestProxy.Create<IStoredFileRepository>((method, _) =>
            method.Name == nameof(IStoredFileRepository.AddAsync)
                ? failOnAdd ? Task.FromException(databaseError) : Task.CompletedTask
                : throw new NotSupportedException(method.Name));
        var unitOfWork = TestProxy.Create<IUnitOfWork>((method, _) =>
            method.Name == nameof(IUnitOfWork.SaveChangesAsync)
                ? Task.FromException<int>(databaseError)
                : throw new NotSupportedException(method.Name));
        var handler = new UploadFileCommandHandler(
            storage,
            repository,
            unitOfWork,
            Options.Create(new FileStorageSettings()),
            NullLogger<UploadFileCommandHandler>.Instance,
            TestProxy.Create<IAuditPublisher>((method, _) => throw new NotSupportedException(method.Name)));

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UploadFileCommand(new MemoryStream([1]), "report.csv", "text/csv", 1),
                CancellationToken.None));

        Assert.Same(databaseError, thrown);
        Assert.True(rollbackAttempted);
    }

}
