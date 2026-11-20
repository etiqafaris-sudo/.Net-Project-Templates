using Microsoft.JSInterop;

namespace CompanyName.Web.Services;

public sealed class AppThemeService
{
    private string _currentTheme = "default";
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _isInitialized = false;

    public AppThemeService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        
        // Try to read the theme from the request cookies during SSR
        var cookieTheme = _httpContextAccessor.HttpContext?.Request?.Cookies["AppTheme"];
        if (!string.IsNullOrEmpty(cookieTheme))
        {
            _currentTheme = cookieTheme;
        }
    }

    public string CurrentTheme => _currentTheme;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var jsRuntime = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
            
            // Read cookie via JS Interop for the actual client
            var cookieStr = await jsRuntime.InvokeAsync<string>("eval", "document.cookie");
            var storedTheme = cookieStr?.Split(';')
                .Select(c => c.Trim())
                .FirstOrDefault(c => c.StartsWith("AppTheme="))
                ?.Substring("AppTheme=".Length);
            
            if (!string.IsNullOrEmpty(storedTheme) && storedTheme != _currentTheme)
            {
                _currentTheme = storedTheme;
                OnChange?.Invoke();
            }
        }
        catch
        {
            // JS Interop might fail during pre-rendering, ignore
        }
        finally
        {
            _isInitialized = true;
        }
    }

    public async Task SetThemeAsync(string theme)
    {
        _currentTheme = theme;
        OnChange?.Invoke();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var jsRuntime = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
            // Set cookie valid for 1 year
            var script = $"document.cookie = 'AppTheme={theme}; max-age=31536000; path=/';";
            await jsRuntime.InvokeVoidAsync("eval", script);
        }
        catch
        {
            // Ignore JS interop errors
        }
    }
}
