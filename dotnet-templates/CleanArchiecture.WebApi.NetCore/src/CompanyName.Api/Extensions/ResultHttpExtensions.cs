using CompanyName.Shared;

namespace CompanyName.Api.Extensions;

internal static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem(
            detail: result.Error,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Request failed");
    }

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return Results.Problem(
            detail: result.Error,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Request failed");
    }
}