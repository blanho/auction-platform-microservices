using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.Users.Queries.GetAdminStats;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;

public record GetAdminStatsQuery() : IQuery<AdminStatsResponse>;

public class GetAdminStatsQueryHandler(
    UserManager<ApplicationUser> userManager) : IQueryHandler<GetAdminStatsQuery, AdminStatsResponse>
{
    public async Task<Result<AdminStatsResponse>> Handle(GetAdminStatsQuery query, CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        var stats = await userManager.Users
            .GroupBy(_ => 1)
            .Select(g => new AdminStatsResponse
            {
                TotalUsers = g.Count(),
                ActiveUsers = g.Count(u => u.IsActive),
                SuspendedUsers = g.Count(u => u.IsSuspended),
                NewUsersThisMonth = g.Count(u => u.CreatedAt >= thirtyDaysAgo)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success(stats ?? new AdminStatsResponse());
    }
}
