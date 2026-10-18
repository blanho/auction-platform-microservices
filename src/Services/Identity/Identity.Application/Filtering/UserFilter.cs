using BuildingBlocks.Application.Filtering;
using Identity.Domain.Entities;

namespace Identity.Application.Filtering;

public sealed class UserFilter : IFilter<ApplicationUser>
{
    public string? Search { get; init; }
    public string? RoleId { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsSuspended { get; init; }

    public IQueryable<ApplicationUser> Apply(IQueryable<ApplicationUser> query) =>
        FilterBuilder<ApplicationUser>.Create()
            .WhenNotEmpty(Search, user =>
                EF.Functions.ILike(user.UserName!, $"%{Search}%") ||
                EF.Functions.ILike(user.Email!, $"%{Search}%") ||
                (user.FullName != null && EF.Functions.ILike(user.FullName, $"%{Search}%")))
            .WhenNotEmpty(RoleId, user => user.UserRoles.Any(role => role.RoleId == RoleId))
            .WhenHasValue(IsActive, user => user.IsActive == IsActive!.Value)
            .WhenHasValue(IsSuspended, user => user.IsSuspended == IsSuspended!.Value)
            .Apply(query);
}
