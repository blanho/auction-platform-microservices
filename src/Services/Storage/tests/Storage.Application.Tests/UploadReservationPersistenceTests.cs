using Microsoft.EntityFrameworkCore;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using Storage.Infrastructure.Persistence;
using TestSupport;
using Xunit;

namespace Storage.Application.Tests;

public class UploadReservationPersistenceTests
{
    [PostgresFact]
    public async Task ConcurrentConfirmationCannotClaimReservationTwice()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<StorageDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using (var seed = new StorageDbContext(options))
        {
            await seed.Database.MigrateAsync();
            seed.StoredFiles.Add(StoredFile.ReserveUpload(Guid.NewGuid(), "image.png", "key.png", "image/png",
                10, "images", Guid.NewGuid(), StorageProvider.AzureBlob, DateTimeOffset.UtcNow.AddMinutes(10)));
            await seed.SaveChangesAsync();
        }
        await using var first = new StorageDbContext(options);
        await using var second = new StorageDbContext(options);
        var a = await first.StoredFiles.SingleAsync();
        var b = await second.StoredFiles.SingleAsync();
        a.ConfirmUpload("https://storage/key.png");
        b.ConfirmUpload("https://storage/key.png");
        await first.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        await using var check = new StorageDbContext(options);
        Assert.Equal(1, await check.StoredFiles.CountAsync());
        Assert.Equal(FileStatus.Ready, (await check.StoredFiles.SingleAsync()).Status);
    }
}
