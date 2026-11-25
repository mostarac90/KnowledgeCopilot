using KnowledgeCopilot.Infrastructure.Database;
using Npgsql;
using Pgvector.Npgsql;

namespace KnowledgeCopilot.Api.Extensions;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddPostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Postgres connection string is missing.");

        var dataSourceBuilder =
            new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.UseVector();

        services.AddSingleton(dataSourceBuilder.Build());

        services.AddSingleton(sp => new DatabaseMigrator(
            connectionString,
            sp.GetRequiredService<ILogger<DatabaseMigrator>>()));

        return services;
    }
}
