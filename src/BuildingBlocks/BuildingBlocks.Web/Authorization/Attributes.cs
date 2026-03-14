using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Web.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(string permission) : AuthorizeAttribute($"Permission:{permission}")
{
    public string Permission { get; } = permission;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireAdminAttribute : AuthorizeAttribute
{
    public RequireAdminAttribute() : base()
    {
        Roles = Authorization.Roles.Admin;
    }

    public RequireAdminAttribute(string? _) : this() { }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(string role)
    {
        Roles = role;
    }

    public RequireRoleAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}
