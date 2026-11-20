using Microsoft.JSInterop;

namespace CompanyName.Web.Services;

public sealed class BrowserNavigationService(IJSRuntime jsRuntime)
{
    public Task ForceNavigateAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return jsRuntime.InvokeVoidAsync("forceNavigate", cancellationToken, url).AsTask();
        }
        catch (JSDisconnectedException)
        {
            return Task.CompletedTask;
        }
        catch (TaskCanceledException)
        {
            return Task.CompletedTask;
        }
    }
}
