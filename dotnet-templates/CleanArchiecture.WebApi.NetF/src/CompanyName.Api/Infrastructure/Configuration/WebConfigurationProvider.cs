using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SysConfig = System.Configuration;

namespace CompanyName.Api.Infrastructure.Configuration;

/// <summary>
/// Maps <c>Web.config</c> <see cref="ConnectionStrings"/> and <see cref="AppSettings"/> into <see cref="IConfiguration"/> keys.
/// </summary>
internal sealed class WebConfigurationProvider : ConfigurationProvider
{
    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (SysConfig.ConfigurationManager.ConnectionStrings is { } connectionStrings)
        {
            foreach (SysConfig.ConnectionStringSettings connectionString in connectionStrings)
            {
                if (string.IsNullOrEmpty(connectionString.Name))
                {
                    continue;
                }

                data[$"ConnectionStrings:{connectionString.Name}"] = connectionString.ConnectionString;
            }
        }

        if (SysConfig.ConfigurationManager.AppSettings is { } appSettings)
        {
            foreach (string? key in appSettings.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                data[key] = appSettings[key];
            }
        }

        Data = data!;
    }
}