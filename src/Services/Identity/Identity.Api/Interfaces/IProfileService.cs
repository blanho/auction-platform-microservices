using BuildingBlocks.Application.Abstractions;
using Identity.Api.DTOs.Auth;
using Identity.Api.DTOs.Profile;

namespace Identity.Api.Interfaces;

public interface IProfileService
{
    Task<Result<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> EnableTwoFactorAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> DisableTwoFactorAsync(string userId, CancellationToken cancellationToken = default);
}
