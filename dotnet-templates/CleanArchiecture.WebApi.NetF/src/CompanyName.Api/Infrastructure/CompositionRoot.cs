using CompanyName.Api.Controllers;
using CompanyName.Api.Controllers.Uam;
using CompanyName.Api.Infrastructure.Configuration;
using CompanyName.Application;
using CompanyName.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CompanyName.Api.Infrastructure;

public static class CompositionRoot
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static IConfiguration Configuration { get; private set; } = null!;

    public static void Initialize()
    {
        ConfigurationFileBootstrap.Initialize();

        Configuration = new ConfigurationBuilder()
            .AddWebConfig()
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration)
            .CreateLogger();

        var services = new ServiceCollection();
        services.AddSingleton(Configuration);
        services.AddLogging(builder => builder.AddSerilog(dispose: true));
        services.AddApplication();
        services.AddInfrastructure(Configuration);

        services.AddTransient<UamController>();
        services.AddTransient<HealthController>();

        ServiceProvider = services.BuildServiceProvider();
        HostedServiceRunner.Initialize(ServiceProvider);
    }
}