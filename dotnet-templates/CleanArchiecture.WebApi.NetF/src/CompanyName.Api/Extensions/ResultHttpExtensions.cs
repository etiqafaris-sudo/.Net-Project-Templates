using System;
using System.Web.Http;
using CompanyName.Shared;

namespace CompanyName.Api.Extensions;

internal static class ResultHttpExtensions
{
    public static IHttpActionResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return Results.Problem("Request failed", result.Error, 400);
    }

    public static IHttpActionResult ToHttpResult<T>(this Result<T> result, Func<T, IHttpActionResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return Results.Problem("Request failed", result.Error, 400);
    }
}