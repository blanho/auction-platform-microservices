namespace Identity.Application.Features.Auth.Commands.Register;

using Identity.Application.DTOs.Auth;
using Identity.Application.DTOs.External;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.DTOs.Users;
using Identity.Application.Features.Auth.Helpers;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record RegisterCommand(RegisterRequest Request) : ICommand<UserDto>;

public class RegisterCommandHandler(
    IUserService userService,
    IMediator mediator,
    IAuditPublisher auditPublisher,
    IMapper mapper,
    IConfiguration configuration,
    ILogger<RegisterCommandHandler> logger) : ICommandHandler<RegisterCommand, UserDto>
{
    private string RequiredFrontendUrl =>
        configuration["FrontendUrl"]
            ?? throw new InvalidOperationException("FrontendUrl configuration is required");

    public async Task<Result<UserDto>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var existingUser = await userService.FindByNameAsync(request.Username);
        if (existingUser != null)
            return Result.Failure<UserDto>(IdentityErrors.Auth.UsernameExists);

        existingUser = await userService.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result.Failure<UserDto>(IdentityErrors.Auth.EmailExists);

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = false
        };

        var result = await userService.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("User registration failed for {Username}: {Errors}", request.Username, string.Join(", ", errors));
            return Result.Failure<UserDto>(IdentityErrors.WithIdentityErrors("Auth.RegistrationFailed", "Registration failed", errors));
        }

        await userService.EnsureRoleExistsAsync(Roles.User);
        var roleResult = await userService.AddToRoleAsync(user, Roles.User);
        if (!roleResult.Succeeded)
        {
            logger.LogWarning("Failed to assign default role to {Username}", request.Username);
        }

        var confirmationToken = await userService.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = EmailLinkHelper.GenerateConfirmationLink(RequiredFrontendUrl, user.Id, confirmationToken);

        await mediator.Publish(new UserCreatedDomainEvent
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = false,
            FullName = user.FullName,
            Role = Roles.User,
            ConfirmationLink = confirmationLink
        });

        await auditPublisher.PublishAsync(
            Guid.Parse(user.Id),
            UserAuditData.FromUser(user, [Roles.User]),
            AuditAction.Created);

        logger.LogInformation("User {Username} registered successfully, awaiting email confirmation", request.Username);

        var userDto = mapper.Map<UserDto>(user);
        userDto.Roles = [Roles.User];

        return Result.Success(userDto);
    }
}
