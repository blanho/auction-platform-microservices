using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Features.Users.Queries.GetUsers;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;
using Identity.Application.Filters;

public record GetUsersListQuery(Identity.Application.DTOs.Users.GetUsersQuery Query) : IQuery<PaginatedResult<AdminUserDto>>;

public class GetUsersListQueryHandler(
    Microsoft.AspNetCore.Identity.UserManager<Identity.Domain.Entities.ApplicationUser> userManager,
    Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager,
    AutoMapper.IMapper mapper,
    ILogger<GetUsersListQueryHandler> logger) : IQueryHandler<GetUsersListQuery, PaginatedResult<AdminUserDto>>
{
    public async Task<Result<PaginatedResult<AdminUserDto>>> Handle(GetUsersListQuery request, CancellationToken cancellationToken)
    {
        var query = request.Query;
        string? roleId = null;
        if (!string.IsNullOrWhiteSpace(query.Filter?.Role))
        {
            roleId = await roleManager.Roles
                .AsNoTracking()
                .Where(r => r.Name == query.Filter.Role)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var dbQuery = userManager.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ApplyUserFilters(query.Filter?.Search, query.Filter?.IsActive, query.Filter?.IsSuspended)
            .ApplyRoleFilter(roleId)
            .ApplySorting(query, Identity.Application.Filters.UserSortMap.Map, u => u.CreatedAt);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var users = await dbQuery
            .ApplyPaging(query)
            .ToListAsync(cancellationToken);

        var roleIds = users.SelectMany(u => u.UserRoles).Select(ur => ur.RoleId).Distinct().ToList();
        var rolesDictionary = new Dictionary<string, string>();
        if (roleIds.Count > 0)
        {
            rolesDictionary = await roleManager.Roles
                .AsNoTracking()
                .Where(r => roleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name!, cancellationToken);
        }

        var dtos = users.Select(user =>
        {
            var dto = mapper.Map<AdminUserDto>(user);
            dto.Roles = user.UserRoles
                .Select(ur => rolesDictionary.GetValueOrDefault(ur.RoleId))
                .OfType<string>()
                .ToList();
            return dto;
        }).ToList();

        return Result.Success(new PaginatedResult<AdminUserDto>(
            dtos,
            totalCount,
            query.Page,
            query.PageSize));
    }
}
