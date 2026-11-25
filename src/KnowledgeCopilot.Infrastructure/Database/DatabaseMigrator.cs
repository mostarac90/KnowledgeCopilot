using System.Reflection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace KnowledgeCopilot.Infrastructure.Database;

/// <summary>
/// Applies the SQL scripts in /database (embedded as resources named
/// "Migrations.*.sql") in filename order. Each script runs once; applied
/// scripts are recorded in the schema_migrations table.
/// </summary>
public sealed class DatabaseMigrator(
    string connectionString,
    ILogger<DatabaseMigrator> logger)
{
    private const string ResourcePrefix = "Migrations.";

    // Arbitrary constant so concurrent app instances don't migrate at the same time.
    private const long AdvisoryLockKey = 4_242_001;

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        // Use a dedicated connection rather than the app's NpgsqlDataSource:
        // that data source loads the pgvector type on first use, which would
        // fail (or be cached as missing) if it opened before the extension exists.
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await ExecuteAsync(connection, $"SELECT pg_advisory_lock({AdvisoryLockKey})", cancellationToken);

        try
        {
            await ExecuteAsync(connection,
                """
                CREATE TABLE IF NOT EXISTS schema_migrations
                (
                    script_name TEXT PRIMARY KEY,
                    applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                )
                """,
                cancellationToken);

            var applied = await GetAppliedScriptsAsync(connection, cancellationToken);
            var pending = GetScripts().Where(s => !applied.Contains(s.Name)).ToList();

            if (pending.Count == 0)
            {
                logger.LogInformation("Database schema is up to date.");
                return;
            }

            foreach (var (name, sql) in pending)
            {
                logger.LogInformation("Applying database migration {Script}", name);

                await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                await using (var command = new NpgsqlCommand(sql, connection, transaction))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await using (var journal = new NpgsqlCommand(
                    "INSERT INTO schema_migrations (script_name) VALUES (@name)",
                    connection, transaction))
                {
                    journal.Parameters.AddWithValue("name", name);
                    await journal.ExecuteNonQueryAsync(cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);
            }

            logger.LogInformation("Applied {Count} database migration(s).", pending.Count);
        }
        finally
        {
            await ExecuteAsync(connection, $"SELECT pg_advisory_unlock({AdvisoryLockKey})", CancellationToken.None);
        }
    }

    private static IEnumerable<(string Name, string Sql)> GetScripts()
    {
        var assembly = typeof(DatabaseMigrator).Assembly;

        foreach (var resource in assembly.GetManifestResourceNames()
                     .Where(r => r.StartsWith(ResourcePrefix, StringComparison.Ordinal)
                              && r.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                     .Order(StringComparer.Ordinal))
        {
            using var stream = assembly.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);
            yield return (resource[ResourcePrefix.Length..], reader.ReadToEnd());
        }
    }

    private static async Task<HashSet<string>> GetAppliedScriptsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        var applied = new HashSet<string>(StringComparer.Ordinal);

        await using var command = new NpgsqlCommand("SELECT script_name FROM schema_migrations", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            applied.Add(reader.GetString(0));
        }

        return applied;
    }

    private static async Task ExecuteAsync(
        NpgsqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
