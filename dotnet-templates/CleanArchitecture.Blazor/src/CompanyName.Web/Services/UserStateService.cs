using System.Security.Claims;
using CompanyName.Web.Models.Users;

namespace CompanyName.Web.Services;

public sealed class UserStateService
{
    public UserStateDto? CurrentUser { get; private set; }

    public event Action? OnChange;

    public void EnsureInitialized(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            if (CurrentUser is not null)
            {
                ClearUser();
            }

            return;
        }

        if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            return;
        }

        var email = principal.FindFirstValue(ClaimTypes.Email);
        var fullname = principal.FindFirstValue(ClaimTypes.Name);
        var icNo = principal.FindFirstValue("ic_no");
        var role = principal.FindFirstValue(ClaimTypes.Role);

        if (CurrentUser is not null
            && CurrentUser.Id == id
            && CurrentUser.Email == email
            && CurrentUser.Fullname == fullname
            && CurrentUser.IcNo == icNo
            && CurrentUser.Role == role)
        {
            return;
        }

        SetUser(new UserStateDto
        {
            Id = id,
            Email = email,
            Fullname = fullname,
            IcNo = icNo,
            Role = role,
        });
    }

    public void SetUser(UserStateDto user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void ClearUser()
    {
        CurrentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
