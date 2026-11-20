using System.Reflection;

namespace CompanyName.Infrastructure.Data;

/// <summary>
/// Loads <c>IBM.Data.Informix.dll</c> from the application output directory (copied from repo <c>lib/</c>),
/// not from an IBM client SDK install or the GAC.
/// </summary>
public static class InformixAssemblyBootstrap
{
    private static int _registered;

    public static void RegisterAssemblyResolver()
    {
        if (Interlocked.Exchange(ref _registered, 1) == 1)
        {
            return;
        }

        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            var assemblyName = new AssemblyName(args.Name).Name;
            if (!string.Equals(assemblyName, "IBM.Data.Informix", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return LoadBundledInformixAssembly();
        };
    }

    internal static Assembly? LoadBundledInformixAssembly()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "IBM.Data.Informix.dll");
        if (!File.Exists(path))
        {
            return null;
        }

        return Assembly.LoadFrom(path);
    }
}
