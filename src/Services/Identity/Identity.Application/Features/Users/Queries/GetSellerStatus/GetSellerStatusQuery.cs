namespace Identity.Application.Features.Users.Queries.GetSellerStatus;

using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS.Commands;
using BuildingBlocks.Application.CQRS.Queries;
using Identity.Application.DTOs.Users;
using Identity.Application.DTOs.Seller;
using Identity.Application.Interfaces;
using BuildingBlocks.Application.Paging;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetSellerStatusQuery(string UserId) : IQuery<SellerStatusResponse>;

public class GetSellerStatusQueryHandler(
    IUserService userService,
    ILogger<GetSellerStatusQueryHandler> logger) : IQueryHandler<GetSellerStatusQuery, SellerStatusResponse>
{
    public async Task<Result<SellerStatusResponse>> Handle(GetSellerStatusQuery query, CancellationToken cancellationToken)
    {
        var (user, roles) = await userService.GetByIdWithRolesAsync(query.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<SellerStatusResponse>(Identity.Application.Errors.IdentityErrors.User.NotFound);

        var isSeller = roles.Contains(Roles.Seller) || roles.Contains(Roles.Admin);

        return Result.Success(new SellerStatusResponse
        {
            IsSeller = isSeller,
            CanBecomeSeller = !isSeller && roles.Contains(Roles.User),
            Roles = roles.ToList()
        });
    }
}
