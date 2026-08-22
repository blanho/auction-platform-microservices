namespace Identity.Application.Features.Auth.Commands.ChangePassword;

using Identity.Application.DTOs.Auth;

public record ChangePasswordCommand(string UserId, ChangePasswordRequest Request) : ICommand;

public class ChangePasswordCommandHandler(
    IUserService userService,
    ILogger<ChangePasswordCommandHandler> logger) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var userId = command.UserId;
        var request = command.Request;

        var user = await userService.FindByIdAsync(userId);
        if (user == null)
            return Result.Failure(IdentityErrors.User.NotFound);

        var result = await userService.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, string.Join(", ", errors));
            return Result.Failure(IdentityErrors.WithIdentityErrors("Profile.PasswordChangeFailed", "Password change failed", errors));
        }

        logger.LogInformation("Password changed successfully for user {UserId}", userId);
        return Result.Success();
    }
}
