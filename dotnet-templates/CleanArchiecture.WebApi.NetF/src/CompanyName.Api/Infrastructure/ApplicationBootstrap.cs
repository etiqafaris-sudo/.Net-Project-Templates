using System.Web.Http;
using CompanyName.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace CompanyName.Api.Infrastructure;

internal static class ApplicationBootstrap
{
    public static void Configure(HttpConfiguration config)
    {
        CompositionRoot.Initialize();
        WebApiConfig.Register(config);
    }

    public static void StartBackgroundWorkers()
    {
        HostedServiceRunner.Instance.Start();
    }

    public static HttpConfiguration CreateHttpConfiguration()
    {
        var config = new HttpConfiguration();
        Configure(config);
        return config;
    }

    public static void Shutdown()
    {
        HostedServiceRunner.Instance.Stop();
        (CompositionRoot.ServiceProvider as IDisposable)?.Dispose();
    }

    public static void TryApplyMigrations()
    {
        try
        {
            var connectionString = CompositionRoot.Configuration.GetConnectionString("TemplateDatabase");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                DatabaseMigrationRunner.ApplyMigrations(connectionString!);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Database migration on startup failed: {0}", ex);
        }
    }
}
