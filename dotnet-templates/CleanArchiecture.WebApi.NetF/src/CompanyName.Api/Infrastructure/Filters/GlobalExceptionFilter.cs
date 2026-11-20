using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http.Filters;
using CompanyName.Application.Common.Exceptions;
using FluentValidation;

namespace CompanyName.Api.Infrastructure.Filters;

internal sealed class GlobalExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(HttpActionExecutedContext context)
    {
        if (context.Exception is null)
        {
            return;
        }

        var (statusCode, title, detail) = MapException(context.Exception);

        var body = new
        {
            title,
            detail,
            status = (int)statusCode,
            instance = context.Request.RequestUri?.AbsolutePath,
        };

        context.Response = context.Request.CreateResponse(
            statusCode,
            body,
            new JsonMediaTypeFormatter());
    }

    private static (HttpStatusCode StatusCode, string Title, string? Detail) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Validation failed",
                string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage))),
            ConcurrencyConflictException => (
                HttpStatusCode.Conflict,
                "Conflict",
                "The record was modified by another user."),
            _ => (
                HttpStatusCode.InternalServerError,
                "An error occurred while processing your request.",
                HttpContext.Current?.IsDebuggingEnabled == true ? exception.Message : null),
        };
    }
}
