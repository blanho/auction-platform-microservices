using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Bidding.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bidding.Infrastructure.Persistence;

public class PostgresAuctionBidLock : IAuctionBidLock
{
    private readonly BidDbContext _context;

    public PostgresAuctionBidLock(BidDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteAsync<T>(
        Guid auctionId,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        var lockKey = ToAdvisoryLockKey(auctionId);
        var closeConnection = _context.Database.GetDbConnection().State == System.Data.ConnectionState.Closed;

        if (closeConnection)
            await _context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_lock({lockKey})",
                cancellationToken);

            try
            {
                return await operation(cancellationToken);
            }
            finally
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_unlock({lockKey})",
                    CancellationToken.None);
            }
        }
        finally
        {
            if (closeConnection)
                await _context.Database.CloseConnectionAsync();
        }
    }

    private static long ToAdvisoryLockKey(Guid auctionId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(auctionId.ToString("N")));
        return BinaryPrimitives.ReadInt64LittleEndian(bytes);
    }
}
