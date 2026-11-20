using System.Data;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared.Configurations;
using Microsoft.Extensions.Options;

namespace CompanyName.Infrastructure.Data;

internal sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly IOptionsMonitor<AppSettings> _appSettings;
    private readonly IEncryptionService _encryption;

    public DbConnectionFactory(IOptionsMonitor<AppSettings> appSettings, IEncryptionService encryption)
    {
        _appSettings = appSettings;
        _encryption = encryption;
    }

    public IDbConnection CreateConnection(string name)
    {
        var entry = FindEntry(name)
                    ?? throw new InvalidOperationException(
                        $"AppSettings:DbConnections entry with Db_Name '{name}' was not found.");

        return CreateConnection(entry.Provider, name);
    }

    public IDbConnection CreateConnection(DatabaseProvider provider, string connectionStringName)
    {
        var entry = FindEntry(connectionStringName);
        if (entry is not null)
        {
            provider = entry.Provider;
            var connectionString = ResolveConnectionString(entry.ConnectionString, _encryption);
            return AdoNetConnections.Create(provider, connectionString);
        }

        throw new InvalidOperationException(
            $"AppSettings:DbConnections entry with Db_Name '{connectionStringName}' was not found.");
    }

    private DbConnections? FindEntry(string dbName) =>
        _appSettings.CurrentValue.DbConnections
            .FirstOrDefault(c => string.Equals(c.Db_Name, dbName, StringComparison.OrdinalIgnoreCase));

    private static string ResolveConnectionString(string encryptedConnectionString, IEncryptionService encryption)
    {
        if (string.IsNullOrWhiteSpace(encryptedConnectionString))
        {
            throw new InvalidOperationException("ConnectionString is missing or empty in AppSettings:DbConnections.");
        }

        return encryption.Decrypt(encryptedConnectionString);
    }
}
