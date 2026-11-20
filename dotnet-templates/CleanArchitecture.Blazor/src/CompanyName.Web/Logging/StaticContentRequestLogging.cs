namespace CompanyName.Web.Logging;

internal static class StaticContentRequestLogging
{
    private static readonly string[] ExcludedPathPrefixes =
    [
        "/_content",
        "/_framework",
    ];

    private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".js",
        ".css",
        ".map",
        ".woff",
        ".woff2",
        ".ttf",
        ".otf",
        ".eot",
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".webp",
        ".svg",
        ".ico",
    };

    public static bool IsStaticContent(HttpRequest request)
    {
        var path = request.Path.Value;
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        if (path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var prefix in ExcludedPathPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        var extension = Path.GetExtension(path);
        return !string.IsNullOrEmpty(extension) && ExcludedExtensions.Contains(extension);
    }
}
