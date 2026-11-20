using System;
using System.Net;
using CompanyName.Api.Infrastructure;
using CompanyName.Api.Infrastructure.Configuration;
using Microsoft.Owin.Hosting;

namespace CompanyName.Api;

public static class Program
{
    public static bool IsSelfHosted { get; private set; }

    public static void Main(string[] args)
    {
        try
        {
            Run(args);
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 5)
        {
            Console.Error.WriteLine(
                "HTTP listener access denied. Use http://localhost:5080, the IIS Express profile, " +
                "or run scripts\\Add-HttpUrlAcl.ps1 as Administrator.");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Environment.Exit(1);
        }
    }

    private static void Run(string[] args)
    {
        IsSelfHosted = true;
        ConfigurationFileBootstrap.Initialize();

        var baseUrl = NormalizeSelfHostBaseUrl(
            Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
                ?? Environment.GetEnvironmentVariable("API_URL")
                ?? "http://localhost:5080/");

        using (WebApp.Start<Startup>(baseUrl))
        {
            ApplicationBootstrap.StartBackgroundWorkers();
            Console.WriteLine($"Listening on {baseUrl.TrimEnd('/')} (press Enter to stop).");
            Console.ReadLine();
        }

        ApplicationBootstrap.Shutdown();
    }

    private static string NormalizeSelfHostBaseUrl(string url)
    {
        url = url.Trim();
        if (url.Contains("://+:") || url.Contains("://*:"))
        {
            throw new InvalidOperationException(
                "Wildcard URLs (http://+:port) are not supported for local SelfHost. Use http://localhost:5080 or IIS Express.");
        }

        if (!url.EndsWith("/", StringComparison.Ordinal))
        {
            url += "/";
        }

        return url;
    }
}