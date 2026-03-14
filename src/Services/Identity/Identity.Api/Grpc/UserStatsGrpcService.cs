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

        var stats = await _dbContext.Users
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalUsers = g.Count(),
                ActiveUsers = g.Count(u => u.IsActive && !u.IsSuspended),
                VerifiedUsers = g.Count(u => u.EmailConfirmed),
                NewUsersToday = g.Count(u => u.CreatedAt.Date == today),
                NewUsersThisWeek = g.Count(u => u.CreatedAt >= weekStart),
                NewUsersThisMonth = g.Count(u => u.CreatedAt >= monthStart)
            })
            .FirstOrDefaultAsync(context.CancellationToken);

        if (stats == null)
        {
            return new PlatformUserStatsResponse();
        }

        return new PlatformUserStatsResponse
        {
            TotalUsers = stats.TotalUsers,
            ActiveUsers = stats.ActiveUsers,
            NewUsersToday = stats.NewUsersToday,
            NewUsersThisWeek = stats.NewUsersThisWeek,
            NewUsersThisMonth = stats.NewUsersThisMonth,
            VerifiedUsers = stats.VerifiedUsers
        };
    }
}
