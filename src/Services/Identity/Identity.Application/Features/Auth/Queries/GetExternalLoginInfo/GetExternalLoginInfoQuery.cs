namespace Identity.Application.Features.Auth.Queries.GetExternalLoginInfo;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

public record GetExternalLoginInfoQuery() : IQuery<ExternalLoginInfo?>;

public class GetExternalLoginInfoQueryHandler(
    SignInManager<ApplicationUser> signInManager) : IQueryHandler<GetExternalLoginInfoQuery, ExternalLoginInfo?>
{
    public async Task<Result<ExternalLoginInfo?>> Handle(GetExternalLoginInfoQuery query, CancellationToken cancellationToken)
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        return Result.Success(info);
    }
}
