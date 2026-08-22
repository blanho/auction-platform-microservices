using System.Security.Claims;
using BuildingBlocks.Application.Authorization;
using BuildingBlocks.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;

namespace Identity.Application.Tests;

public sealed class PermissionHandlerTests
{
    [Fact]
    public async Task HandleAsync_SucceedsForIssuedPermissionClaim()
    {
        var requirement = new PermissionRequirement(Permissions.Users.View);
        var context = CreateContext(requirement,
            new Claim(AuthClaimTypes.Permission, Permissions.Users.View));

        await new PermissionHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_DoesNotInferPermissionFromAdminRole()
    {
        var requirement = new PermissionRequirement(Permissions.Users.View);
        var context = CreateContext(requirement,
            new Claim(AuthClaimTypes.Role, Roles.Admin));

        await new PermissionHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_RejectsDifferentPermissionClaim()
    {
        var requirement = new PermissionRequirement(Permissions.Users.Delete);
        var context = CreateContext(requirement,
            new Claim(AuthClaimTypes.Permission, Permissions.Users.View));

        await new PermissionHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void HasPermission_UsesIssuedPermissionClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AuthClaimTypes.Role, Roles.User),
            new Claim(AuthClaimTypes.Permission, Permissions.Users.Delete)
        ], "test"));

        Assert.True(principal.HasPermission(Permissions.Users.Delete));
        Assert.False(principal.HasPermission(Permissions.Auctions.View));
    }

    [Fact]
    public async Task PermissionPolicy_RequiresAuthenticatedUser()
    {
        var provider = new PermissionPolicyProvider(Options.Create(new AuthorizationOptions()));

        var policy = await provider.GetPolicyAsync($"Permission:{Permissions.Users.View}");

        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);
        Assert.Contains(policy.Requirements,
            requirement => requirement is PermissionRequirement permissionRequirement &&
                           permissionRequirement.Permission == Permissions.Users.View);
    }

    private static AuthorizationHandlerContext CreateContext(
        PermissionRequirement requirement,
        params Claim[] claims)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return new AuthorizationHandlerContext([requirement], principal, resource: null);
    }
}
