using System.Data;
using System.Data.Common;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Shared.Configurations;

namespace CompanyName.Infrastructure.Data;

public static class DbConnectionFactoryExtensions
{
    /// <summary>Opens the connection so it is ready for Dapper queries.</summary>
    public static async Task<IDbConnection> CreateOpenConnectionAsync(
        this IDbConnectionFactory factory,
        string name,
        CancellationToken cancellationToken = default)
    {
        var connection = factory.CreateConnection(name);
        await OpenAsync(connection, cancellationToken);
        return connection;
    }

    public static async Task<IDbConnection> CreateOpenConnectionAsync(
        this IDbConnectionFactory factory,
        DatabaseProvider provider,
        string connectionStringName,
        CancellationToken cancellationToken = default)
    {
        var connection = factory.CreateConnection(provider, connectionStringName);
        await OpenAsync(connection, cancellationToken);
        return connection;
    }

    private static async Task OpenAsync(IDbConnection connection, CancellationToken cancellationToken)
    {
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync(cancellationToken);
            return;
        }

        connection.Open();
    }
}
