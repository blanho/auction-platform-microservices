using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using Identity.Api.DomainEvents;
using Identity.Api.DTOs.Audit;
using Identity.Api.DTOs.Auth;
using Identity.Api.DTOs.Profile;
using Identity.Api.Errors;
using Identity.Api.Interfaces;
using Identity.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly IAuditPublisher _auditPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        UserManager<ApplicationUser> userManager,
        IUserService userService,
        IMediator mediator,
        IAuditPublisher auditPublisher,
        IMapper mapper,
        ILogger<ProfileService> logger)
    {
        _userManager = userManager;
        _userService = userService;
        _mediator = mediator;
        _auditPublisher = auditPublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var (user, roles) = await _userService.GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<UserProfileDto>(IdentityErrors.User.NotFound);

        var profile = _mapper.Map<UserProfileDto>(user);
        profile.Roles = roles.ToList();

        return Result.Success(profile);
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var (user, roles) = await _userService.GetByIdWithRolesAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure<UserProfileDto>(IdentityErrors.User.NotFound);

        var oldUserData = UserAuditData.FromUser(user, roles);

        user.FullName = request.FullName ?? user.FullName;
        user.Bio = request.Bio ?? user.Bio;
        user.Location = request.Location ?? user.Location;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result.Failure<UserProfileDto>(IdentityErrors.WithIdentityErrors("Failed to update profile", result.Errors));

        _logger.LogInformation("User {UserId} updated their profile", userId);

        var profile = _mapper.Map<UserProfileDto>(user);
        profile.Roles = roles.ToList();

        await _mediator.Publish(new UserUpdatedDomainEvent
        {
            UserId = profile.Id,
            Username = profile.Username,
            Email = profile.Email,
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber
        });

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, roles),
            AuditAction.Updated,
            oldUserData,
            new Dictionary<string, object> { ["action"] = "profile_update" },
            cancellationToken);

        return Result.Success(profile);
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
            return Result.Failure(IdentityErrors.WithIdentityErrors("Failed to change password", result.Errors));

        await _mediator.Publish(new PasswordChangedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!
        }, cancellationToken);

        await _auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            new AuthAuditData
            {
                UserId = user.Id,
                Username = user.UserName,
                Action = "PasswordChange",
                Success = true
            },
            AuditAction.Updated,
            metadata: new Dictionary<string, object> { ["action"] = "password_change" },
            cancellationToken: cancellationToken);

        _logger.LogInformation("User {UserId} changed their password", userId);

        return Result.Success();
    }

    public async Task<Result> EnableTwoFactorAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        await _userManager.SetTwoFactorEnabledAsync(user, true);

        _logger.LogInformation("User {UserId} enabled 2FA", userId);

        return Result.Success();
    }

    public async Task<Result> DisableTwoFactorAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        await _userManager.SetTwoFactorEnabledAsync(user, false);

        _logger.LogInformation("User {UserId} disabled 2FA", userId);

        return Result.Success();
    }
}
