using System.Linq.Expressions;
using Identity.Domain.Entities;

namespace Identity.Application.Filters;

public static class UserSortMap
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<ApplicationUser, object>>> Map =
        new Dictionary<string, Expression<Func<ApplicationUser, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAt"] = u => u.CreatedAt,
            ["username"] = u => u.UserName!,
            ["email"] = u => u.Email!,
            ["fullName"] = u => u.FullName ?? string.Empty,
            ["lastLoginAt"] = u => u.LastLoginAt ?? DateTimeOffset.MinValue,
            ["isActive"] = u => u.IsActive,
            ["isSuspended"] = u => u.IsSuspended
        };
}
