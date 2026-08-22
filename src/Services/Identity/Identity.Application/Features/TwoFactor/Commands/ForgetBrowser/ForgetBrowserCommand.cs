namespace Identity.Application.Features.TwoFactor.Commands.ForgetBrowser;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.TwoFactor;
using Identity.Application.Interfaces;
using MediatR;

public record ForgetBrowserCommand() : ICommand;

public class ForgetBrowserCommandHandler(
    SignInManager<ApplicationUser> signInManager) : ICommandHandler<ForgetBrowserCommand>
{
    public async Task<Result> Handle(ForgetBrowserCommand command, CancellationToken cancellationToken)
    {
        await signInManager.ForgetTwoFactorClientAsync();
        return Result.Success();
    }
}
