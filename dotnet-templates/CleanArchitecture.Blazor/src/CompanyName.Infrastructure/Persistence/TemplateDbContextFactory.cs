using CompanyName.Infrastructure.Services;
using CompanyName.Shared.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CompanyName.Infrastructure.Persistence;

public sealed class TemplateDbContextFactory : IDesignTimeDbContextFactory<TemplateDbContext>
{
    public TemplateDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()
            ?? throw new InvalidOperationException("AppSettings section is missing from configuration.");

        var defaultDb = appSettings.DbConnections.FirstOrDefault(x =>
            x.Db_Name.Equals("TemplateDatabase", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("TemplateDatabase connection was not found in AppSettings.");

        var encryption = new AesEncryptionService(appSettings);
        var connectionString = encryption.Decrypt(defaultDb.ConnectionString);

        var optionsBuilder = new DbContextOptionsBuilder<TemplateDbContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            b => b.MigrationsAssembly(typeof(TemplateDbContext).Assembly.FullName));

        return new TemplateDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var webProjectPath = ResolveWebProjectPath();

        return new ConfigurationBuilder()
            .SetBasePath(webProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static string ResolveWebProjectPath()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            if (directory.Name.Equals("CompanyName.Web", StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
            {
                return directory.FullName;
            }

            var webPath = Path.Combine(directory.FullName, "src", "CompanyName.Web");
            if (Directory.Exists(webPath) && File.Exists(Path.Combine(webPath, "appsettings.json")))
            {
                return webPath;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate CompanyName.Web appsettings.json for design-time migrations.");
    }
}
