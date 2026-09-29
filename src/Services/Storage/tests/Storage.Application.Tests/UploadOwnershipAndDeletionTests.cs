using System.Reflection;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Storage.Application.Features.Files.ConfirmPresignedUpload;
using Storage.Application.Features.Files.DeleteFile;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using Xunit;

namespace Storage.Application.Tests;

public class UploadOwnershipAndDeletionTests
{
    [Theory]
    [InlineData("owner")]
    [InlineData("folder")]
    [InlineData("expired")]
    [InlineData("unissued")]
    public async Task ConfirmationRejectsUnissuedExpiredOrAlteredReservation(string change)
    {
        var owner = Guid.NewGuid();
        var reservation = StoredFile.ReserveUpload(Guid.NewGuid(), "image.png", "images/key.png",
            "image/png", 10, "images", owner, StorageProvider.AzureBlob,
            DateTimeOffset.UtcNow.AddMinutes(change == "expired" ? -1 : 10));
        var repository = Stub<IStoredFileRepository>((method, _) => method.Name switch
        {
            nameof(IStoredFileRepository.GetByStoredFileNameAsync) => Task.FromResult(change == "unissued" ? null : reservation),
            _ => throw new InvalidOperationException("Unexpected database write")
        });
        var storage = Stub<IFileStorageService>((_, _) => throw new InvalidOperationException("Unexpected blob access"));
        var handler = new ConfirmPresignedUploadCommandHandler(storage, repository, null!,
            NullLogger<ConfirmPresignedUploadCommandHandler>.Instance, null!);
        var request = new ConfirmPresignedUploadCommand("images/key.png", "image.png", "image/png", 10,
            change == "folder" ? "other" : "images", change == "owner" ? Guid.NewGuid() : owner);
        Assert.True((await handler.Handle(request, CancellationToken.None)).IsFailure);
    }

    [Fact]
    public async Task ConfirmationRetryReturnsSameFileAndOnlySavesOnce()
    {
        var owner = Guid.NewGuid();
        var reservation = StoredFile.ReserveUpload(Guid.NewGuid(), "image.png", "images/key.png",
            "image/png", 10, "images", owner, StorageProvider.AzureBlob, DateTimeOffset.UtcNow.AddMinutes(10));
        var saves = 0;
        var repository = Stub<IStoredFileRepository>((method, _) => method.Name switch
        {
            nameof(IStoredFileRepository.GetByStoredFileNameAsync) => Task.FromResult<StoredFile?>(reservation),
            nameof(IStoredFileRepository.Update) => null,
            _ => throw new NotSupportedException(method.Name)
        });
        var storage = Stub<IFileStorageService>((method, _) => method.Name switch
        {
            nameof(IFileStorageService.ExistsAsync) => Task.FromResult(true),
            nameof(IFileStorageService.GetUrlAsync) => Task.FromResult<string?>("https://storage/images/key.png"),
            _ => throw new NotSupportedException(method.Name)
        });
        var uow = Stub<IUnitOfWork>((_, _) => { saves++; return Task.FromResult(1); });
        var audit = Stub<IAuditPublisher>((_, _) => Task.CompletedTask);
        var handler = new ConfirmPresignedUploadCommandHandler(storage, repository, uow,
            NullLogger<ConfirmPresignedUploadCommandHandler>.Instance, audit);
        var request = new ConfirmPresignedUploadCommand("images/key.png", "image.png", "image/png", 10, "images", owner);
        var first = await handler.Handle(request, CancellationToken.None);
        var retry = await handler.Handle(request, CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Equal(first.Value!.FileId, retry.Value!.FileId);
        Assert.Equal(reservation.Id, first.Value!.FileId);
        Assert.Equal(1, saves);
    }

    [Fact]
    public async Task FailedDatabaseDeleteNeverTouchesBlob()
    {
        var file = File();
        var repository = Repository(file);
        var uow = Stub<IUnitOfWork>((_, _) => Task.FromException<int>(new IOException("database unavailable")));
        var storage = Stub<IFileStorageService>((_, _) => throw new InvalidOperationException("Blob must remain intact"));
        var handler = new DeleteFileCommandHandler(repository, storage, uow,
            NullLogger<DeleteFileCommandHandler>.Instance, null!);
        await Assert.ThrowsAsync<IOException>(() => handler.Handle(new DeleteFileCommand(file.Id, file.OwnerId), CancellationToken.None));
    }

    [Fact]
    public async Task FailedBlobDeleteLeavesPersistedDeletionForCleanup()
    {
        var file = File();
        var saved = false;
        var uow = Stub<IUnitOfWork>((_, _) => { Assert.True(file.IsDeleted); saved = true; return Task.FromResult(1); });
        var storage = Stub<IFileStorageService>((_, _) =>
        {
            Assert.True(saved);
            return Task.FromException<bool>(new IOException("blob unavailable"));
        });
        var handler = new DeleteFileCommandHandler(Repository(file), storage, uow,
            NullLogger<DeleteFileCommandHandler>.Instance, Stub<IAuditPublisher>((_, _) => Task.CompletedTask));
        Assert.True((await handler.Handle(new DeleteFileCommand(file.Id, file.OwnerId), CancellationToken.None)).IsSuccess);
        Assert.True(file.IsDeleted);
    }

    [Fact]
    public async Task RepeatedReportUploadReturnsStoredIdentityWithoutUploadingAgain()
    {
        var file = File();
        var requestId = Guid.NewGuid();
        file.SetReportRequestId(requestId);
        var repository = Stub<IStoredFileRepository>((method, args) =>
        {
            Assert.Equal(nameof(IStoredFileRepository.GetByReportRequestIdAsync), method.Name);
            Assert.Equal(file.OwnerId, args![0]);
            Assert.Equal(requestId, args[1]);
            return Task.FromResult<StoredFile?>(file);
        });
        var storage = Stub<IFileStorageService>((_, _) => throw new InvalidOperationException("Unexpected second upload"));
        var handler = new Features.Files.UploadFile.UploadFileCommandHandler(
            storage, repository, null!, null!,
            NullLogger<Features.Files.UploadFile.UploadFileCommandHandler>.Instance, null!);
        var request = new Features.Files.UploadFile.UploadFileCommand(
            Stream.Null, "report.json", "application/json", 10, OwnerId: file.OwnerId, ReportRequestId: requestId);
        var result = await handler.Handle(request, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal(file.Id, result.Value!.FileId);
    }

    private static StoredFile File() => StoredFile.Create("image.png", "key.png", "image/png", 10,
        "/files/key.png", null, Guid.NewGuid(), StorageProvider.Local);
    private static IStoredFileRepository Repository(StoredFile file) => Stub<IStoredFileRepository>((method, _) => method.Name switch
    {
        nameof(IStoredFileRepository.GetByIdAsync) => Task.FromResult<StoredFile?>(file),
        nameof(IStoredFileRepository.Update) => null,
        _ => throw new NotSupportedException(method.Name)
    });
    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, Proxy>();
        ((Proxy)(object)proxy).Handler = handler;
        return proxy;
    }
    public class Proxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    }
}
