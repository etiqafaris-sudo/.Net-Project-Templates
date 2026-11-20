namespace CompanyName.Shared.Configurations;

/// <summary>
/// Maps <see cref="DatabaseProvider"/> (config / app logic) to ADO.NET connection types.
/// Use enum names in appsettings; use this when resolving provider assemblies at runtime.
/// </summary>
public static class DatabaseProviderAdoNet
{
    public readonly record struct ConnectionTypeNames(string Primary, string Fallback, string Description);

    public static ConnectionTypeNames GetConnectionTypeNames(DatabaseProvider provider) =>
        provider switch
        {
            DatabaseProvider.SqlServer => new ConnectionTypeNames(
                Primary: "Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient",
                Fallback: "System.Data.SqlClient.SqlConnection, System.Data.SqlClient",
                Description: "SQL Server (Microsoft.Data.SqlClient or legacy System.Data.SqlClient)."),
            DatabaseProvider.Informix => new ConnectionTypeNames(
                Primary: "IBM.Data.Informix.IfxConnection, IBM.Data.Informix",
                Fallback: "IBM.Data.Informix.IfxConnection, IBM.Data.Informix",
                Description: "Informix ADO.NET provider (IBM.Data.Informix from repo lib/)."),
            DatabaseProvider.MySql => new ConnectionTypeNames(
                Primary: "MySqlConnector.MySqlConnection, MySqlConnector",
                Fallback: "MySql.Data.MySqlClient.MySqlConnection, MySql.Data",
                Description: "MySQL provider (install MySqlConnector or MySql.Data)."),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unsupported database provider."),
        };
}
