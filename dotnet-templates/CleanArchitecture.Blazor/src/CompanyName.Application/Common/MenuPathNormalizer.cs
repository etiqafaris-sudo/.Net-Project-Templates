namespace CompanyName.Application.Common;

public static class MenuPathNormalizer
{
    public static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var value = path.Trim();
        if (!value.StartsWith('/'))
        {
            value = "/" + value;
        }

        value = value.Split('?', '#')[0];

        if (value.Length > 1 && value.EndsWith('/'))
        {
            value = value.TrimEnd('/');
        }

        return value.ToLowerInvariant();
    }

    public static bool IsUnderMenuPath(string requestPath, string menuUrl)
    {
        var normalizedRequest = NormalizePath(requestPath);
        var normalizedMenu = NormalizePath(menuUrl);

        if (normalizedMenu == "/")
        {
            return normalizedRequest == "/";
        }

        return normalizedRequest == normalizedMenu
            || normalizedRequest.StartsWith(normalizedMenu + "/", StringComparison.Ordinal);
    }
}
