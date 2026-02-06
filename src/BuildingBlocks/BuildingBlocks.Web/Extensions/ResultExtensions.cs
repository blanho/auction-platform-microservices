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

        return result.Error!.Code.EndsWith(".NotFound")
            ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error));
    }

    public static IResult ToApiResult(this Result result, Func<IResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess();

        return result.Error!.Code.EndsWith(".NotFound")
            ? Results.NotFound(ProblemDetailsHelper.FromError(result.Error))
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error));
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
}
