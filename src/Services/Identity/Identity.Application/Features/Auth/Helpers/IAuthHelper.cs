namespace Identity.Application.Features.Auth.Helpers;

using Identity.Application.DTOs.Auth;
using Identity.Domain.Entities;

public interface IAuthHelper
{
    Task<Result<LoginResponse>> GenerateLoginResponseAsync(ApplicationUser user, string ipAddress);
    Task<string> GenerateUniqueUsernameAsync(string baseName);
    Task PublishEmailEventAsync(string userId, string email, string name, string templateKey, string subject, Dictionary<string, string> data);
    Task<ApplicationUser?> FindUserByUsernameOrEmailAsync(string usernameOrEmail);
}
