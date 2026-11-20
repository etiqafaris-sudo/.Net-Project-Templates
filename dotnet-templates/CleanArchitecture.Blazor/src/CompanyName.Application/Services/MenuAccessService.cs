using CompanyName.Application.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Common.Models;
using CompanyName.Application.Features.Menus.Queries.GetAllMenuByUser;
using CompanyName.Application.Features.Menus.Queries.GetAllMenus;
using Mediator;
using System.Security.Claims;

namespace CompanyName.Application.Services;

internal sealed class MenuAccessService(IMediator mediator) : IMenuAccessService
{
    private static readonly string[] SkipPathPrefixes =
    [
        "/_framework",
        "/_blazor",
        "/_content",
        "/api/auth",
        "/images",
        "/css",
        "/js",
        "/favicon",
    ];

    private static readonly HashSet<string> AnonymousPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/",
        "/login",
    };

    private static readonly HashSet<string> AuthenticatedWhitelist = new(StringComparer.OrdinalIgnoreCase)
    {
        "/profile",
        "/logout",
        "/dashboard",
        "/access-denied",
        "/not-found",
        "/error",
        "/home",
    };

    public bool ShouldSkipPathCheck(string path)
    {
        var normalized = MenuPathNormalizer.NormalizePath(path);

        if (AnonymousPaths.Contains(normalized))
        {
            return true;
        }

        foreach (var prefix in SkipPathPrefixes)
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return HasStaticFileExtension(normalized);
    }

    public bool IsWhitelistedPath(string path)
    {
        var normalized = MenuPathNormalizer.NormalizePath(path);
        return AuthenticatedWhitelist.Contains(normalized);
    }

    public async Task<bool> CanAccessPathAsync(
        ClaimsPrincipal user,
        string path,
        CancellationToken cancellationToken = default)
    {
        if (ShouldSkipPathCheck(path))
        {
            return true;
        }

        if (user.Identity?.IsAuthenticated != true)
        {
            return true;
        }

        var normalizedPath = MenuPathNormalizer.NormalizePath(path);

        if (IsWhitelistedPath(normalizedPath))
        {
            return true;
        }

        var allMenusResult = await mediator.Send(new GetAllMenusQuery(), cancellationToken);
        var allMenus = allMenusResult.IsSuccess ? allMenusResult.Value : Array.Empty<GetAllMenusReponse>();

        var registeredMenuUrls = allMenus
            .Where(m => m.IsActive == 1 && !string.IsNullOrWhiteSpace(m.MenuUrl))
            .Select(m => MenuPathNormalizer.NormalizePath(m.MenuUrl))
            .Distinct()
            .ToList();

        var matchesRegisteredMenu = registeredMenuUrls.Any(url => MenuPathNormalizer.IsUnderMenuPath(normalizedPath, url));
        if (!matchesRegisteredMenu)
        {
            return true;
        }

        if (!int.TryParse(user.FindFirstValue(ClaimTypes.Role), out var groupId))
        {
            return false;
        }

        var userMenusResult = await mediator.Send(new GetAllMenuByUserQuery(groupId), cancellationToken);
        var userMenus = userMenusResult.IsSuccess ? userMenusResult.Value : Array.Empty<GetAllMenuByUserResponse>();

        var allowedUrls = userMenus
            .Where(m => !string.IsNullOrWhiteSpace(m.MenuUrl))
            .Select(m => MenuPathNormalizer.NormalizePath(m.MenuUrl))
            .Distinct()
            .ToList();

        return allowedUrls.Any(url => MenuPathNormalizer.IsUnderMenuPath(normalizedPath, url));
    }

    private static bool HasStaticFileExtension(string path)
    {
        var extension = Path.GetExtension(path);
        return extension.Length > 1
            && extension != ".razor"
            && extension != ".cshtml";
    }
}
