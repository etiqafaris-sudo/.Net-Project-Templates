using System.Security.Claims;

namespace CompanyName.Application.Common.Interfaces;

public interface IMenuAccessService
{
    Task<bool> CanAccessPathAsync(ClaimsPrincipal user, string path, CancellationToken cancellationToken = default);

    bool ShouldSkipPathCheck(string path);

    bool IsWhitelistedPath(string path);
}
