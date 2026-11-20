using CompanyName.Application.Features.Auth.Queries.Login;

namespace CompanyName.Application.Common.Interfaces;

public interface ICookieAuthService
{
    Task SignInAsync(LoginQueryResponse user, bool isPersistent = true, CancellationToken cancellationToken = default);

    Task SignOutAsync(CancellationToken cancellationToken = default);
}
