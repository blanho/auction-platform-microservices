using Grpc.Core;
using IdentityService.Contracts.Grpc;
using Identity.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Grpc;

public class UserStatsGrpcService(
    ApplicationDbContext dbContext,
    ILogger<UserStatsGrpcService> logger)
    : UserStatsGrpc.UserStatsGrpcBase
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<UserStatsGrpcService> _logger = logger;

    public override async Task<UserStatsResponse> GetUserStats(
        GetUserStatsRequest request,
        ServerCallContext context)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == request.Username, context.CancellationToken);

        if (user == null)
        {
            return new UserStatsResponse
            {
                UserId = string.Empty,
                Username = request.Username,
                SellerRating = 0,
                ReviewCount = 0,
                MemberSince = string.Empty,
                TotalTransactions = 0
            };
        }

        return new UserStatsResponse
        {
            UserId = user.Id,
            Username = user.UserName ?? request.Username,
            SellerRating = 0,
            ReviewCount = 0,
            MemberSince = user.CreatedAt.ToString("O"),
            TotalTransactions = 0
        };
    }

    public override async Task<PlatformUserStatsResponse> GetPlatformUserStats(
        GetPlatformUserStatsRequest request,
        ServerCallContext context)
    {
        var now = DateTimeOffset.UtcNow;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var query = _dbContext.Users.AsNoTracking();

        var totalUsers = await query.CountAsync(context.CancellationToken);
        var activeUsers = await query.CountAsync(u => u.IsActive && !u.IsSuspended, context.CancellationToken);
        var verifiedUsers = await query.CountAsync(u => u.EmailConfirmed, context.CancellationToken);
        var newUsersToday = await query.CountAsync(u => u.CreatedAt.Date == today, context.CancellationToken);
        var newUsersThisWeek = await query.CountAsync(u => u.CreatedAt >= weekStart, context.CancellationToken);
        var newUsersThisMonth = await query.CountAsync(u => u.CreatedAt >= monthStart, context.CancellationToken);

        return new PlatformUserStatsResponse
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            NewUsersToday = newUsersToday,
            NewUsersThisWeek = newUsersThisWeek,
            NewUsersThisMonth = newUsersThisMonth,
            VerifiedUsers = verifiedUsers
        };
    }
}
