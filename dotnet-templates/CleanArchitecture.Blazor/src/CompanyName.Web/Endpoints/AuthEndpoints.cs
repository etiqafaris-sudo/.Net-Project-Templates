using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Features.Auth.Commands.Logout;
using CompanyName.Application.Features.Auth.Queries.Login;
using Mediator;
using Microsoft.AspNetCore.Antiforgery;
using System.Security.Claims;

namespace CompanyName.Web.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/login", async (
            HttpContext httpContext,
            IAntiforgery antiforgery,
            LoginRequest request,
            IMediator mediator,
            ICookieAuthService cookieAuth,
            CancellationToken cancellationToken) =>
        {
            await antiforgery.ValidateRequestAsync(httpContext);

            var result = await mediator.Send(
                new LoginQuery(request.Email, request.Password),
                cancellationToken);

            if (result.IsFailure)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            var user = result.Value;
            await cookieAuth.SignInAsync(user, isPersistent: true, cancellationToken);

            return Results.Ok(new { fullname = user.Fullname });
        });

        group.MapGet("/logout", async (
            HttpContext httpContext,
            ICookieAuthService cookieAuth,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            int? userId = null;
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                if (int.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                    userId = id;
            }

            await cookieAuth.SignOutAsync(cancellationToken);
            var result = await mediator.Send(new LogoutCommand(userId ?? 0), cancellationToken);
            return Results.Redirect("/login");
        });

        return endpoints;
    }

    private sealed record LoginRequest(string Email, string Password);
}
