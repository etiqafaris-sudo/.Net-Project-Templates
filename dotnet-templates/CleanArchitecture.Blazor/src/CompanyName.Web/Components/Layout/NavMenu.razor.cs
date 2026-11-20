using System.Security.Claims;
using CompanyName.Application.Features.Menus.Queries.GetAllMenuByUser;
using CompanyName.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CompanyName.Web.Components.Layout;

public partial class NavMenu : AppComponentBase<GlPortalMenu>
{
    [Inject]
    public ILogger<NavMenu> Logger { get; set; } = default!;

    [Inject]
    public CompanyName.Web.Services.UserStateService UserState { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    public IMediator Mediator { get; set; } = default!;

    public IReadOnlyList<GetAllMenuByUserResponse>? MenuItems { get; set; } = new List<GetAllMenuByUserResponse>();

    protected override async Task OnInitializedAsync()
    {
        AuthenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        UserState.OnChange += OnUserStateChanged;
        L.OnChange += StateHasChanged;

        try
        {
            await RefreshMenusAsync();
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "NavMenu initialization was canceled.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (authState.User.Identity?.IsAuthenticated != true || MenuItems is not { Count: 0 })
        {
            return;
        }

        try
        {
            await LoadMenusAsync(authState.User);
            StateHasChanged();
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "NavMenu menu reload after first render was canceled.");
        }
    }

    private async void OnAuthenticationStateChanged(Task<AuthenticationState> authStateTask)
    {
        try
        {
            await InvokeAsync(async () =>
            {
                try
                {
                    var authState = await authStateTask;
                    UserState.EnsureInitialized(authState.User);
                    await LoadMenusAsync(authState.User);
                    StateHasChanged();
                }
                catch (OperationCanceledException ex)
                {
                    Logger.LogWarning(ex, "NavMenu auth state update was canceled.");
                }
            });
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void OnUserStateChanged()
    {
        _ = InvokeAsync(async () =>
        {
            try
            {
                await RefreshMenusAsync();
                StateHasChanged();
            }
            catch (OperationCanceledException ex)
            {
                Logger.LogWarning(ex, "NavMenu user state update was canceled.");
            }
        });
    }

    private async Task RefreshMenusAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        UserState.EnsureInitialized(authState.User);
        await LoadMenusAsync(authState.User);
    }

    private async Task LoadMenusAsync(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            MenuItems = Array.Empty<GetAllMenuByUserResponse>();
            return;
        }

        var userGroupId = ResolveUserGroupId(user);
        var result = await Mediator.Send(new GetAllMenuByUserQuery(userGroupId));

        MenuItems = result.Value ?? Array.Empty<GetAllMenuByUserResponse>();
    }

    private int ResolveUserGroupId(ClaimsPrincipal user)
    {
        if (int.TryParse(UserState.CurrentUser?.Role, out var userGroupId))
        {
            return userGroupId;
        }

        if (int.TryParse(user.FindFirstValue(ClaimTypes.Role), out userGroupId))
        {
            return userGroupId;
        }

        return 0;
    }

    protected string GetMenuLabel(GetAllMenuByUserResponse menu)
    {
        if (menu.MenuName != null && menu.MenuName.StartsWith("lbl_"))
        {
            return L.GetLangValue(menu.MenuName);
        }

        return menu.MenuName ?? string.Empty;
    }

    protected static string GetMenuIcon(string? icon)
    {
        if (string.IsNullOrWhiteSpace(icon) || icon.Contains(' ', StringComparison.Ordinal))
        {
            return "link";
        }

        return icon;
    }

    public void Dispose()
    {
        AuthenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        UserState.OnChange -= OnUserStateChanged;
        L.OnChange -= StateHasChanged;
    }
}
