using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using CompanyName.Infrastructure.Persistence.Migrations;

namespace CompanyName.Infrastructure.Persistence;

public static class DatabaseMigrationRunner
{
    public static void ApplyMigrations(string connectionString)
    {
        var configuration = new TemplateDbMigrationsConfiguration
        {
            TargetDatabase = new DbConnectionInfo(connectionString, "System.Data.SqlClient"),
        };

        var migrator = new DbMigrator(configuration);
        migrator.Update();
    }
}