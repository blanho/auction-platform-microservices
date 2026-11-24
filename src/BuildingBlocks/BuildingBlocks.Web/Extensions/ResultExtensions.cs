using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Helpers;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Web.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value!);

        return ToErrorResult(result.Error!);
    }

    public static IResult ToApiResult(this Result result, Func<IResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess();

        return ToErrorResult(result.Error!);
    }

    public static IResult ToNoContentResult<T>(this Result<T> result)
    {
        return result.ToApiResult(_ => Results.NoContent());
    }

    public static IResult ToNoContentResult(this Result result)
    {
        return result.ToApiResult(() => Results.NoContent());
    }

    public static IResult ToOkResult<T>(this Result<T> result)
    {
        return result.ToApiResult(value => Results.Ok(value));
    }

    private static IResult ToErrorResult(Error error)
    {
        var problem = ProblemDetailsHelper.FromError(error);
        return error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? Results.NotFound(problem)
            : Results.BadRequest(problem);
    }
}
