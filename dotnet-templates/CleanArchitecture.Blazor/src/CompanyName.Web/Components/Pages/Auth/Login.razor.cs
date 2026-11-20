using CompanyName.Web.Components.Pages.Shared;
using CompanyName.Web.Models.Login;
using CompanyName.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace CompanyName.Web.Components.Pages.Auth;

public partial class Login : AppComponentBase<LoginDto>
{
    #region ---Declaration---
    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    [Inject]
    public BrowserNavigationService BrowserNavigation { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    #endregion

    #region ---Page Load---
    protected override void OnInitialized()
    {
        L.OnChange += () => InvokeAsync(StateHasChanged);
    }
    #endregion

    #region ---Other Function---
    public void Dispose()
    {
        L.OnChange -= () => InvokeAsync(StateHasChanged);
    }

    #endregion

    #region ---Button---
    public async Task OnForgotPassword()
    {
        var result = await DialogService.OpenAsync<ForgotPasswordDialog>("Forgot Password",
            new Dictionary<string, object?>(),
            new DialogOptions() { Width = "800px", Height = "auto", Resizable = false, Draggable = false });
    }

    public async Task OnSignup()
    {
        var result = await DialogService.OpenAsync<SignupDialog>("Create Account",
            new Dictionary<string, object?>(),
            new DialogOptions() { Width = "800px", Height = "auto", Resizable = false, Draggable = false });
    }

    public async Task OnSubmit(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await RunWithLoadingAsync(async () =>
                await JS.InvokeAsync<LoginJsResult>(
                    "authLogin",
                    cancellationToken,
                    Model.Email,
                    Model.Password),
                messageKey: "lbl_login_loading");

            if (!result.Success)
            {
                await JSErrorAsync(result.Error ?? "Login failed.");
                return;
            }

            await JSSuccessAsync($"Welcome back, {result.Fullname}!");
            await BrowserNavigation.ForceNavigateAsync("/dashboard", cancellationToken);
        }
        catch (Exception ex)
        {
            await JSErrorAsync($"Login error: {ex.Message}");
        }
    }

    private sealed record LoginJsResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Fullname { get; set; }
    }
    #endregion
}
