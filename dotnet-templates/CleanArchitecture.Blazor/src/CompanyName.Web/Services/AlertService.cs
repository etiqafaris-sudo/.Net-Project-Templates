using Microsoft.JSInterop;

namespace CompanyName.Web.Services;

public sealed class AlertService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly LoadingIndicatorService _loadingIndicator;

    public AlertService(IJSRuntime jsRuntime, LoadingIndicatorService loadingIndicator)
    {
        _jsRuntime = jsRuntime;
        _loadingIndicator = loadingIndicator;
    }

    public Task JSAlertAsync(string message) => ShowAlertAsync("JSAlert", message);

    public Task JSWarningAsync(string message) => ShowAlertAsync("JSWarning", message);

    public Task JSSuccessAsync(string message) => ShowAlertAsync("JSSuccess", message);

    public Task JSErrorAsync(string message) => ShowAlertAsync("JSError", message);

    private async Task ShowAlertAsync(string functionName, string message)
    {
        if (_loadingIndicator.IsActive)
        {
            _loadingIndicator.SuppressForInteraction();
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync(functionName, message);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
