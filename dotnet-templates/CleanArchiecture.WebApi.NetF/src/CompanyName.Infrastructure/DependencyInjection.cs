using System;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Infrastructure.Health;
using CompanyName.Infrastructure.Outbox;
using CompanyName.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace CompanyName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TemplateDatabase")
            ?? throw new InvalidOperationException("Connection string 'TemplateDatabase' is not configured.");

        services.AddScoped<TemplateDbContext>(_ => new TemplateDbContext(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<TemplateDbContext>());

        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddScoped<IOutbox, OutboxWriter>();
        services.AddSingleton<OutboxProcessorHostedService>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<OutboxProcessorHostedService>());

        services.AddHealthChecks()
            .AddCheck<SqlServerDatabaseHealthCheck>(
                "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready" });

        return services;
    }
}