using Microsoft.JSInterop;
using System.Xml.Linq;

namespace CompanyName.Web.Services;

public sealed class XmlLocalizationService
{
    private Dictionary<string, string> _resources = new();
    private string _currentCulture = "en";
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _isInitialized = false;

    public XmlLocalizationService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        
        // Try to read the language from the request cookies during SSR
        var cookieCulture = _httpContextAccessor.HttpContext?.Request?.Cookies["AppLanguage"];
        if (!string.IsNullOrEmpty(cookieCulture))
        {
            _currentCulture = cookieCulture;
        }
        
        Load(_currentCulture);
    }

    public string CurrentCulture => _currentCulture;

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var jsRuntime = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
            
            // Read cookie via JS Interop
            var cookieStr = await jsRuntime.InvokeAsync<string>("eval", "document.cookie");
            var storedCulture = cookieStr?.Split(';')
                .Select(c => c.Trim())
                .FirstOrDefault(c => c.StartsWith("AppLanguage="))
                ?.Substring("AppLanguage=".Length);
            
            if (!string.IsNullOrEmpty(storedCulture) && storedCulture != _currentCulture)
            {
                _currentCulture = storedCulture;
                Load(_currentCulture);
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

    public async Task SetCultureAsync(string culture)
    {
        _currentCulture = culture;
        Load(culture);
        OnChange?.Invoke();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var jsRuntime = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
            // Set cookie valid for 1 year (365 days * 24h * 60m * 60s)
            var script = $"document.cookie = 'AppLanguage={culture}; max-age=31536000; path=/';";
            await jsRuntime.InvokeVoidAsync("eval", script);
        }
        catch
        {
            // Ignore JS interop errors
        }
    }

    private void Load(string culture)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/Language/{culture}.xml");
        
        if (File.Exists(path))
        {
            var doc = XDocument.Load(path);

            _resources = doc.Root?
                            .Elements("text")
                            .ToDictionary(x => x.Attribute("key")?.Value ?? "",
                                          x => x.Value) ?? new Dictionary<string, string>();
        }
        else
        {
            // Fallback or empty if file not found
            _resources = new Dictionary<string, string>();
        }
    }

    public string GetLangValue(string key)
    {
        return _resources.TryGetValue(key, out var value) ? value : key;
    }
}
