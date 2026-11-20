using CompanyName.Application.Common.Interfaces;
using CompanyName.Application.Services;
using CompanyName.Infrastructure.Data;
using CompanyName.Infrastructure.Persistence;
using CompanyName.Infrastructure.Persistence.Interceptors;
using CompanyName.Infrastructure.Services;
using CompanyName.Shared.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CompanyName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<AuditInterceptor>();
        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        services.AddDbContext<TemplateDbContext>((sp, options) =>
        {
            var encryption = sp.GetRequiredService<IEncryptionService>();
            var appSettings = sp.GetRequiredService<IOptionsMonitor<AppSettings>>().CurrentValue;
            var currentDb = appSettings.DbConnections.FirstOrDefault(x => x.Db_Name.Equals("TemplateDatabase", StringComparison.CurrentCultureIgnoreCase));
            var decryptedConnectionString = encryption.Decrypt(currentDb.ConnectionString);
            options.UseSqlServer(
                decryptedConnectionString,
                b => b.MigrationsAssembly(typeof(TemplateDbContext).Assembly.FullName));
            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        //// please comment or remove when create new project. ini sample je 😊
        //services.AddDbContext<TestingDbContext>((sp, options) =>
        //{
        //    var encryption = sp.GetRequiredService<IEncryptionService>();
        //    var appSettings = sp.GetRequiredService<IOptionsMonitor<AppSettings>>().CurrentValue;
        //    var currentDb = appSettings.DbConnections.FirstOrDefault(x => x.Db_Name.Equals("TemplateDatabase", StringComparison.CurrentCultureIgnoreCase));
        //    var decryptedConnectionString = encryption.Decrypt(currentDb.ConnectionString);
        //    options.UseSqlServer(
        //        decryptedConnectionString,
        //        b => b.MigrationsAssembly(typeof(TestingDbContext).Assembly.FullName));
        //    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        //});

        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        //multiple db need to add sini
        services.AddScoped(provider => (ITemplateDbContext)provider.GetRequiredService<TemplateDbContext>());
        services.AddScoped(provider => (ITestingDbContext)provider.GetRequiredService<TestingDbContext>());

        services.AddScoped<CompanyName.Application.Common.Interfaces.IDal, CompanyName.Infrastructure.Services.Dal>();

        //services.AddMemoryCache();
        services.AddScoped<GlUserService>();
        services.AddScoped<IParameterService, ParameterService>();

        return services;
    }
}
