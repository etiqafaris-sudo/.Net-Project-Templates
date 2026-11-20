using CompanyName.Infrastructure.Persistence.Seed;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task MigrateTemplateDatabaseAsync(
        TemplateDbContext context,
        CancellationToken cancellationToken = default)
    {
        await EnsureSqlServerDatabaseExistsAsync(context, cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
        await ApplicationSqlScriptSeeder.SeedFromInitScriptIfEmptyAsync(context, cancellationToken);
    }

    private static async Task EnsureSqlServerDatabaseExistsAsync(
        DbContext context,
        CancellationToken cancellationToken)
    {
        if (!context.Database.IsSqlServer())
        {
            return;
        }

        var connection = context.Database.GetDbConnection();
        var databaseName = connection.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            return;
        }

        var masterConnectionString = new SqlConnectionStringBuilder(connection.ConnectionString)
        {
            InitialCatalog = "master",
        }.ConnectionString;

        await using var masterConnection = new SqlConnection(masterConnectionString);
        await masterConnection.OpenAsync(cancellationToken);

        await using var command = masterConnection.CreateCommand();
        command.CommandText = """
            IF DB_ID(@databaseName) IS NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@databaseName);
                EXEC (@sql);
            END
            """;

        command.Parameters.Add(new SqlParameter("@databaseName", databaseName));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
