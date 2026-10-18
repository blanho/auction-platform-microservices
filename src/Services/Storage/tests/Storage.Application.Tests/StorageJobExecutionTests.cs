using System.Security.Cryptography;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Storage;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Storage.Domain.Constants;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using Storage.Domain.Events;
using Storage.Infrastructure.Messaging.Consumers;
using Storage.Infrastructure.Persistence;
using Storage.Infrastructure.Processing;
using StorageService.Contracts.Commands;
using TestSupport;
using Xunit;

namespace Storage.Application.Tests;

public class StorageJobExecutionTests
{
    [PostgresFact]
    public async Task ImageJob_PersistsMetadataAndReplayKeepsOneResult()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.SeedAsync(fixture.ReadyImage());
        var first = Assert.Single(await fixture.ProcessImageAsync());
        Assert.True(first.IsSuccess);
        Assert.Equal(fixture.JobId, first.JobId);
        Assert.Equal(fixture.ItemId, first.JobItemId);
        Assert.Equal(2, first.Attempt);
        await using var read = fixture.Open();
        var stored = await read.StoredFiles.SingleAsync();
        Assert.Equal(Convert.ToHexString(SHA256.HashData(fixture.Bytes)).ToLowerInvariant(), stored.Checksum);
        Assert.Equal("640", stored.Metadata[StorageDefaults.Images.WidthKey]);
        Assert.Equal("320", stored.Metadata[StorageDefaults.Images.HeightKey]);
        var thumbnail = stored.Metadata[StorageDefaults.Images.ThumbnailKey];
        var updatedAt = stored.UpdatedAt;
        Assert.True(Assert.Single(await fixture.ProcessImageAsync()).IsSuccess);
        await read.Entry(stored).ReloadAsync();
        Assert.Equal(thumbnail, stored.Metadata[StorageDefaults.Images.ThumbnailKey]);
        Assert.Equal(updatedAt, stored.UpdatedAt);
        Assert.Equal(1, await read.StoredFiles.CountAsync());
        Assert.Equal(0, fixture.UploadEvents);
    }

    [PostgresFact]
    public async Task ImageJob_RejectsAnotherOwnerAndPrivateFilesBeforeDownload()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.SeedAsync(fixture.ReadyImage());
        Assert.False(Assert.Single(await fixture.ProcessImageAsync(Guid.NewGuid())).IsSuccess);
        Assert.Equal(0, fixture.Downloads);
        await using (var db = fixture.Open())
        {
            var file = await db.StoredFiles.SingleAsync();
            db.Entry(file).Property(x => x.SubFolder).CurrentValue = "private-reports";
            await db.SaveChangesAsync();
        }
        Assert.False(Assert.Single(await fixture.ProcessImageAsync()).IsSuccess);
        Assert.Equal(0, fixture.Downloads);
    }

    [PostgresFact]
    public async Task ImageJob_RejectsCorruptOrPendingFilesWithoutSavingMetadata()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Bytes = [1, 2, 3];
        await fixture.SeedAsync(fixture.ReadyImage());
        Assert.False(Assert.Single(await fixture.ProcessImageAsync()).IsSuccess);
        await using var db = fixture.Open();
        Assert.Null((await db.StoredFiles.SingleAsync()).Checksum);
        Assert.Empty((await db.StoredFiles.SingleAsync()).Metadata);
    }

    [PostgresFact]
    public async Task UploadFinalization_IsIdempotentUnderConcurrentCommands()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.SeedAsync(fixture.Reservation());
        var results = await Task.WhenAll(fixture.FinalizeAsync(), fixture.FinalizeAsync());
        Assert.All(results, r => Assert.True(Assert.Single(r).IsSuccess));
        Assert.Equal(1, fixture.UploadEvents);
        Assert.Equal(1, fixture.Downloads);
        await using var db = fixture.Open();
        var file = await db.StoredFiles.SingleAsync();
        Assert.Equal(FileStatus.Ready, file.Status);
        Assert.Equal("/files/test.png", file.Url);
    }

    [PostgresFact]
    public async Task UploadFinalization_RejectsExpiredForeignAndIncompleteReservations()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.SeedAsync(fixture.Reservation());
        Assert.False(Assert.Single(await fixture.FinalizeAsync(Guid.NewGuid())).IsSuccess);
        Assert.Equal(0, fixture.Downloads);
        fixture.ActualDownloadSize = fixture.Bytes.Length + 1;
        Assert.False(Assert.Single(await fixture.FinalizeAsync()).IsSuccess);
        fixture.ActualDownloadSize = null;
        fixture.DownloadBytes = fixture.Bytes[..^1];
        Assert.False(Assert.Single(await fixture.FinalizeAsync()).IsSuccess);
        fixture.DownloadBytes = [.. fixture.Bytes, 0];
        Assert.False(Assert.Single(await fixture.FinalizeAsync()).IsSuccess);
        await using var db = fixture.Open();
        var file = await db.StoredFiles.SingleAsync();
        Assert.Equal(FileStatus.Pending, file.Status);
        Assert.Equal(0, fixture.UploadEvents);
        db.Entry(file).Property(x => x.UploadExpiresAt).CurrentValue = DateTimeOffset.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();
        var downloads = fixture.Downloads;
        Assert.False(Assert.Single(await fixture.FinalizeAsync()).IsSuccess);
        Assert.Equal(downloads, fixture.Downloads);
    }

    [PostgresFact]
    public async Task StorageExceptionsAndCancellation_PropagateWithoutTerminalResults()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.SeedAsync(fixture.ReadyImage());
        fixture.DownloadFailure = new HttpRequestException("storage unavailable");
        await Assert.ThrowsAsync<HttpRequestException>(() => fixture.ProcessImageAsync());
        Assert.Empty(fixture.Published);
        fixture.DownloadFailure = new OperationCanceledException();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.ProcessImageAsync());
        Assert.Empty(fixture.Published);
        await using var db = fixture.Open();
        Assert.Null((await db.StoredFiles.SingleAsync()).Checksum);
    }

    [PostgresFact]
    public async Task PersistenceFailure_RollsBackImageMetadataAndUploadConfirmation()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.SeedAsync(fixture.ReadyImage());
        fixture.FailSave = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.ProcessImageAsync());
        await using var db = fixture.Open();
        Assert.Null((await db.StoredFiles.SingleAsync()).Checksum);
        await db.StoredFiles.ExecuteDeleteAsync();
        await fixture.SeedAsync(fixture.Reservation());
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.FinalizeAsync());
        db.ChangeTracker.Clear();
        Assert.Equal(FileStatus.Pending, (await db.StoredFiles.SingleAsync()).Status);
    }

    private sealed class Fixture(PostgresTestDatabase database) : IAsyncDisposable
    {
        public Guid Owner { get; } = Guid.NewGuid();
        public Guid FileId { get; private set; }
        public Guid JobId { get; } = Guid.NewGuid();
        public Guid ItemId { get; } = Guid.NewGuid();
        public byte[] Bytes { get; set; } = ImageProcessorTests.ImageBytes(640, 320);
        public byte[]? DownloadBytes { get; set; }
        public long? ActualDownloadSize { get; set; }
        public Exception? DownloadFailure { get; set; }
        public bool FailSave { get; set; }
        public int Downloads;
        public int UploadEvents;
        public List<object> Published { get; } = [];
        public static async Task<Fixture> CreateAsync()
        {
            var fixture = new Fixture(await PostgresTestDatabase.CreateAsync());
            await using var db = fixture.Open();
            await db.Database.MigrateAsync();
            return fixture;
        }
        public StorageDbContext Open() => new(new DbContextOptionsBuilder<StorageDbContext>()
            .UseNpgsql(database.ConnectionString).Options);
        public StoredFile ReadyImage() => StoredFile.Create(new CreateStoredFileParams
        {
            FileName = "test.png", StoredFileName = "test.png", ContentType = "image/png", FileSize = Bytes.Length,
            Url = "/files/test.png", SubFolder = "images", OwnerId = Owner, Provider = StorageProvider.Local
        });
        public StoredFile Reservation() => StoredFile.ReserveUpload(new ReserveStoredFileUploadParams
        {
            Id = Guid.NewGuid(), FileName = "test.png", StoredFileName = "test.png", ContentType = "image/png",
            FileSize = Bytes.Length, SubFolder = "images", OwnerId = Owner, Provider = StorageProvider.Local,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
        });
        public async Task SeedAsync(StoredFile file)
        {
            FileId = file.Id;
            await using var db = Open();
            db.StoredFiles.Add(file);
            await db.SaveChangesAsync();
        }
        public async Task<List<ReportJobItemResultCommand>> ProcessImageAsync(Guid? requester = null)
        {
            await using var db = Open();
            var storage = Storage();
            var results = new List<ReportJobItemResultCommand>();
            var consumer = new ProcessImageConsumer(db, new StorageJobFiles(db, storage, Options.Create(new FileStorageSettings())),
                UnitOfWork(db));
            await consumer.Consume(Context(new ProcessImageCommand
            {
                FileId = FileId, RequestedBy = requester ?? Owner, ParentJobId = JobId, ParentJobItemId = ItemId,
                Attempt = 2, CorrelationId = Guid.NewGuid()
            }, results));
            return results;
        }
        public async Task<List<ReportJobItemResultCommand>> FinalizeAsync(Guid? requester = null)
        {
            await using var db = Open();
            var storage = Storage();
            var options = Options.Create(new FileStorageSettings());
            var results = new List<ReportJobItemResultCommand>();
            var consumer = new FinalizeFileUploadConsumer(db, new StorageJobFiles(db, storage, options), storage, options, UnitOfWork(db));
            await consumer.Consume(Context(new FinalizeFileUploadCommand
            {
                FileId = FileId, RequestedBy = requester ?? Owner, ParentJobId = JobId, ParentJobItemId = ItemId,
                Attempt = 2, CorrelationId = Guid.NewGuid()
            }, results));
            return results;
        }
        private IFileStorageService Storage() => TestProxy.Create<IFileStorageService>((method, _) => method.Name switch
        {
            nameof(IFileStorageService.DownloadAsync) => Download(),
            nameof(IFileStorageService.GetUrlAsync) => Task.FromResult<string?>("/files/test.png"),
            _ => throw new NotSupportedException(method.Name)
        });
        private Task<FileDownloadResult?> Download()
        {
            Interlocked.Increment(ref Downloads);
            if (DownloadFailure is not null) return Task.FromException<FileDownloadResult?>(DownloadFailure);
            return Task.FromResult<FileDownloadResult?>(new FileDownloadResult(new MemoryStream(DownloadBytes ?? Bytes),
                "test.png", "image/png", ActualDownloadSize ?? Bytes.Length));
        }
        private IUnitOfWork UnitOfWork(StorageDbContext db) => TestProxy.Create<IUnitOfWork>((method, args) =>
        {
            if (method.Name != nameof(IUnitOfWork.SaveChangesAsync)) throw new NotSupportedException(method.Name);
            if (FailSave) return Task.FromException<int>(new InvalidOperationException("persistence unavailable"));
            Interlocked.Add(ref UploadEvents, db.ChangeTracker.Entries<StoredFile>()
                .Sum(e => e.Entity.DomainEvents.OfType<FileUploadedDomainEvent>().Count()));
            return db.SaveChangesAsync((CancellationToken)args![0]!);
        });
        private ConsumeContext<T> Context<T>(T command, List<ReportJobItemResultCommand> results) where T : class =>
            TestProxy.Create<ConsumeContext<T>>((method, args) => method.Name switch
            {
                "get_Message" => command,
                "get_CancellationToken" => CancellationToken.None,
                "Publish" => Capture(args![0]!, results),
                _ => throw new NotSupportedException(method.Name)
            });
        private Task Capture(object value, List<ReportJobItemResultCommand> results)
        {
            lock (Published) Published.Add(value);
            results.Add(Assert.IsType<ReportJobItemResultCommand>(value));
            return Task.CompletedTask;
        }
        public ValueTask DisposeAsync() => database.DisposeAsync();
    }
}
