using CompanyName.Application.Common.Interfaces;
using CompanyName.Infrastructure.Health;
using CompanyName.Infrastructure.Outbox;
using CompanyName.Infrastructure.Persistence;
using CompanyName.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CompanyName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddSingleton<InsertOutboxMessagesInterceptor>();

        //faris : pakai utk connection pooling, utk readonly
        //var poolSize = configuration.GetValue("Database:DbContextPoolSize", 128);

        services.AddDbContext<TemplateDbContext>(
            (sp, options) =>
            {
                var connectionString = configuration.GetConnectionString("TemplateDatabase")
                    ?? throw new InvalidOperationException("Connection string 'TemplateDatabase' is not configured.");

                options.UseSqlServer(
                    connectionString,
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(TemplateDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                options.AddInterceptors(
                    sp.GetRequiredService<AuditableEntityInterceptor>(),
                    sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
            });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<TemplateDbContext>());

        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));
        services.AddHostedService<OutboxProcessorBackgroundService>();

        services.AddHealthChecks()
            .AddCheck<SqlServerDatabaseHealthCheck>(
                "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        return services;
    }
}
