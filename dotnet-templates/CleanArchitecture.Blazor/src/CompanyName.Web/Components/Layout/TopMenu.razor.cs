using CompanyName.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace CompanyName.Web.Components.Layout;

public partial class TopMenu
{
    [Inject]
    public UserStateService UserState { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private string searchText = "";

    protected string UserDisplayName =>
        !string.IsNullOrWhiteSpace(UserState.CurrentUser?.Fullname)
            ? UserState.CurrentUser!.Fullname!
            : UserState.CurrentUser?.Email ?? string.Empty;

    protected string UserEmail => UserState.CurrentUser?.Email ?? string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        UserState.EnsureInitialized(authState.User);

        L.OnChange += StateHasChanged;
        UserState.OnChange += OnUserStateChanged;
    }

    private void OnUserStateChanged() => _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        L.OnChange -= StateHasChanged;
        UserState.OnChange -= OnUserStateChanged;
    }

    private void OnSearch(ChangeEventArgs args)
    {
        searchText = args.Value?.ToString() ?? "";
    }
}
