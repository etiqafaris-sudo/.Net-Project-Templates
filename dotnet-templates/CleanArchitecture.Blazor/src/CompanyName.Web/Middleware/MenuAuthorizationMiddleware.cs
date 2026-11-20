using CompanyName.Application.Common.Interfaces;

namespace CompanyName.Web.Middleware;

public sealed class MenuAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IMenuAccessService menuAccess)
    {
        var path = context.Request.Path.Value ?? "/";

        if (menuAccess.ShouldSkipPathCheck(path))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        if (await menuAccess.CanAccessPathAsync(context.User, path, context.RequestAborted))
        {
            await next(context);
            return;
        }

        context.Response.Redirect("/access-denied");
    }
}

public static class MenuAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseMenuAuthorization(this IApplicationBuilder app) =>
        app.UseMiddleware<MenuAuthorizationMiddleware>();
}
