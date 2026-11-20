using System;
using System.IO;

namespace CompanyName.Api.Infrastructure.Configuration;

/// <summary>
/// Ensures <see cref="System.Configuration.ConfigurationManager"/> reads <c>Web.config</c> when the API runs as a console executable (OWIN self-host).
/// </summary>
internal static class ConfigurationFileBootstrap
{
    private static bool _initialized;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web.config");
        if (File.Exists(configPath))
        {
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configPath);
        }

        _initialized = true;
    }
}