using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace CompanyName.Infrastructure.Persistence.Seed;

public static class ApplicationSqlScriptSeeder
{
    public static async Task SeedFromInitScriptIfEmptyAsync(
        TemplateDbContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.Database.IsSqlServer())
        {
            return;
        }

        if (await context.GlParameters.AnyAsync(cancellationToken))
        {
            return;
        }

        var script = ReadInitScript();
        var batches = SplitSqlBatches(script);

        await context.Database.OpenConnectionAsync(cancellationToken);
        var connection = context.Database.GetDbConnection();

        try
        {
            foreach (var batch in batches)
            {
                if (ShouldSkipBatch(batch))
                {
                    continue;
                }

                await using var command = connection.CreateCommand();
                command.CommandText = batch;
                command.CommandTimeout = 120;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private static string ReadInitScript()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("initScripts.sql", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                "Embedded SQL script initScripts.sql was not found in CompanyName.Infrastructure.");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IEnumerable<string> SplitSqlBatches(string script) =>
        Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private static bool ShouldSkipBatch(string batch)
    {
        var trimmed = batch.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return true;
        }

        if (trimmed.Contains("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return trimmed.Contains("IF NOT EXISTS", StringComparison.OrdinalIgnoreCase)
            && trimmed.Contains("sys.objects", StringComparison.OrdinalIgnoreCase);
    }
}
