using System.Security.Claims;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

public class SeedData
{
    private const string DefaultSeedPassword = "SeedUser@Dev123!";
    
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var seedPassword = configuration["SeedData:Password"] ?? DefaultSeedPassword;

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            
            var admin = userMgr.FindByNameAsync("admin").Result;
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true,
                };
                var result = userMgr.CreateAsync(admin, seedPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(admin, new Claim[]{
                            new Claim(ClaimTypes.Name, "Admin User"),
                            new Claim(ClaimTypes.GivenName, "Admin"),
                            new Claim(ClaimTypes.Surname, "User"),
                            new Claim(ClaimTypes.Role, "admin"),
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("admin created");
            }
            else
            {
                Log.Debug("admin already exists");
            }

            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice",
                    Email = "AliceSmith@example.com",
                    EmailConfirmed = true,
                };
                var result = userMgr.CreateAsync(alice, seedPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(alice, new Claim[]{
                            new Claim(ClaimTypes.Name, "Alice Smith"),
                            new Claim(ClaimTypes.GivenName, "Alice"),
                            new Claim(ClaimTypes.Surname, "Smith"),
                            new Claim(ClaimTypes.Webpage, "http://alice.example.com"),
                            new Claim(ClaimTypes.Role, "user"),
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("alice created");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob",
                    Email = "BobSmith@example.com",
                    EmailConfirmed = true
                };
                var result = userMgr.CreateAsync(bob, seedPassword).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(bob, new Claim[]{
                            new Claim(ClaimTypes.Name, "Bob Smith"),
                            new Claim(ClaimTypes.GivenName, "Bob"),
                            new Claim(ClaimTypes.Surname, "Smith"),
                            new Claim(ClaimTypes.Webpage, "http://bob.example.com"),
                            new Claim(ClaimTypes.Role, "user"),
                            new Claim("location", "somewhere")
                        }).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("bob created");
            }
            else
            {
                Log.Debug("bob already exists");
            }
        }
    }
}
