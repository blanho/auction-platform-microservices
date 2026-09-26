using System.Reflection;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Storage.Application.Features.Files.UploadMultipleFiles;
using Storage.Application.Interfaces;
using Xunit;

namespace Storage.Application.Tests;

public class UploadMultipleFilesFailureTests
{
    [Fact]
    public async Task Handle_PropagatesCancellationDuringUpload()
    {
        using var cancellation = new CancellationTokenSource();
        var storage = Stub<IFileStorageService>((method, _) =>
        {
            Assert.Equal(nameof(IFileStorageService.UploadAsync), method.Name);
            cancellation.Cancel();
            return Task.FromException<FileUploadResult>(new OperationCanceledException(cancellation.Token));
        });
        var handler = CreateHandler(storage);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            handler.Handle(CreateCommand(), cancellation.Token));
    }

    [Fact]
    public async Task Handle_PreservesDatabaseErrorWhenBlobRollbackFails()
    {
        var databaseError = new InvalidOperationException("database failed");
        var deleted = false;
        var storage = Stub<IFileStorageService>((method, args) => method.Name switch
        {
            nameof(IFileStorageService.UploadAsync) => Task.FromResult(new FileUploadResult(
                "id", "report.csv", "stored.csv", "text/csv", 1, "/files/stored.csv", DateTimeOffset.UtcNow)),
            nameof(IFileStorageService.DeleteAsync) => Delete(args),
            _ => throw new NotSupportedException(method.Name)
        });
        Task<bool> Delete(object?[]? args)
        {
            Assert.Equal(CancellationToken.None, args![1]);
            deleted = true;
            return Task.FromException<bool>(new InvalidOperationException("storage failed"));
        }

        var repository = Stub<IStoredFileRepository>((method, _) =>
            method.Name == nameof(IStoredFileRepository.AddRangeAsync)
                ? Task.CompletedTask
                : throw new NotSupportedException(method.Name));
        var unitOfWork = Stub<IUnitOfWork>((method, _) =>
            method.Name == nameof(IUnitOfWork.SaveChangesAsync)
                ? Task.FromException<int>(databaseError)
                : throw new NotSupportedException(method.Name));
        var handler = CreateHandler(storage, repository, unitOfWork);

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(CreateCommand(), CancellationToken.None));

        Assert.Same(databaseError, thrown);
        Assert.True(deleted);
    }

    private static UploadMultipleFilesCommand CreateCommand() => new(
        [new UploadFileItem(new MemoryStream([1]), "report.csv", "text/csv", 1)]);

    private static UploadMultipleFilesCommandHandler CreateHandler(
        IFileStorageService storage,
        IStoredFileRepository? repository = null,
        IUnitOfWork? unitOfWork = null)
    {
        return new UploadMultipleFilesCommandHandler(
            storage,
            repository ?? Stub<IStoredFileRepository>((method, _) => throw new NotSupportedException(method.Name)),
            unitOfWork ?? Stub<IUnitOfWork>((method, _) => throw new NotSupportedException(method.Name)),
            Options.Create(new FileStorageSettings()),
            NullLogger<UploadMultipleFilesCommandHandler>.Instance,
            Stub<IAuditPublisher>((method, _) => throw new NotSupportedException(method.Name)));
    }

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
