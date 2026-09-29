using System.Reflection;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Storage.Application.Features.Files.UploadFile;
using Storage.Application.Interfaces;
using Xunit;

namespace Storage.Application.Tests;

public class UploadFileFailureTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Handle_PreservesDatabaseErrorAndAttemptsBlobRollback(bool failOnAdd)
    {
        var databaseError = new InvalidOperationException("database failed");
        var rollbackAttempted = false;
        var storage = Stub<IFileStorageService>((method, args) => method.Name switch
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

        var repository = Stub<IStoredFileRepository>((method, _) =>
            method.Name == nameof(IStoredFileRepository.AddAsync)
                ? failOnAdd ? Task.FromException(databaseError) : Task.CompletedTask
                : throw new NotSupportedException(method.Name));
        var unitOfWork = Stub<IUnitOfWork>((method, _) =>
            method.Name == nameof(IUnitOfWork.SaveChangesAsync)
                ? Task.FromException<int>(databaseError)
                : throw new NotSupportedException(method.Name));
        var handler = new UploadFileCommandHandler(
            storage,
            repository,
            unitOfWork,
            Options.Create(new FileStorageSettings()),
            NullLogger<UploadFileCommandHandler>.Instance,
            Stub<IAuditPublisher>((method, _) => throw new NotSupportedException(method.Name)));

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new UploadFileCommand(new MemoryStream([1]), "report.csv", "text/csv", 1),
                CancellationToken.None));

        Assert.Same(databaseError, thrown);
        Assert.True(rollbackAttempted);
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
