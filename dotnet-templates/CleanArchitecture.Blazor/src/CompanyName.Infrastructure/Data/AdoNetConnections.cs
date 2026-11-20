using CompanyName.Shared.Configurations;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace CompanyName.Infrastructure.Data;

internal static class AdoNetConnections
{
    public static IDbConnection Create(DatabaseProvider provider, string connectionString)
    {
        if (provider == DatabaseProvider.SqlServer)
        {
            return new SqlConnection(connectionString);
        }

        var typeNames = DatabaseProviderAdoNet.GetConnectionTypeNames(provider);
        return CreateFromAssembly(
            connectionString,
            typeNames.Primary,
            typeNames.Fallback,
            typeNames.Description);
    }

    private static IDbConnection CreateFromAssembly(
        string connectionString,
        string primaryTypeName,
        string fallbackTypeName,
        string providerDescription)
    {
        var type = ResolveConnectionType(primaryTypeName)
                   ?? ResolveConnectionType(fallbackTypeName);

        if (type is null)
        {
            throw new InvalidOperationException(
                $"{providerDescription} Ensure IBM.Data.Informix.dll from the repo lib folder is copied to the application output directory.");
        }

        return (IDbConnection)Activator.CreateInstance(type, connectionString)!;
    }

    private static Type? ResolveConnectionType(string assemblyQualifiedName)
    {
        var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
        if (type is not null)
        {
            return type;
        }

        var commaIndex = assemblyQualifiedName.IndexOf(',');
        if (commaIndex < 0)
        {
            return null;
        }

        var typeName = assemblyQualifiedName[..commaIndex].Trim();
        var assemblyName = assemblyQualifiedName[(commaIndex + 1)..].Trim();
        if (!string.Equals(assemblyName, "IBM.Data.Informix", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var assembly = InformixAssemblyBootstrap.LoadBundledInformixAssembly();
        return assembly?.GetType(typeName, throwOnError: false);
    }
}
