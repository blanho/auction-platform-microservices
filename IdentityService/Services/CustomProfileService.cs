using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Duende.IdentityServer.Extensions;

namespace IdentityService.Services;

public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var subjectId = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(subjectId);
        if (user == null)
        {
            return;
        }

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id),
            new Claim("preferred_username", user.UserName ?? string.Empty),
            new Claim("name", user.UserName ?? string.Empty),
            new Claim("email", user.Email ?? string.Empty)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var userClaims = await _userManager.GetClaimsAsync(user);
        var roleClaims = userClaims.Where(c => c.Type == "role");
        foreach (var roleClaim in roleClaims)
        {
            if (!claims.Any(c => c.Type == "role" && c.Value == roleClaim.Value))
            {
                claims.Add(new Claim("role", roleClaim.Value));
            }
        }

        var requested = claims.Where(c => context.RequestedClaimTypes.Contains(c.Type)).ToList();
        if (!requested.Any())
        {
            requested = claims;
        }
        context.IssuedClaims.AddRange(requested);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var subjectId = context.Subject.GetSubjectId();
        var user = await _userManager.FindByIdAsync(subjectId);
        context.IsActive = user != null && !await _userManager.IsLockedOutAsync(user);
    }
}
