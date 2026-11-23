using Npgsql;

namespace KnowledgeCopilot.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health");

        group.MapGet("/", GetHealth);
        group.MapGet("/database", GetDatabaseHealth);

        return app;
    }

    private static IResult GetHealth()
    {
        return Results.Ok(new
        {
            Status = "Healthy",
            Service = "KnowledgeCopilot.Api",
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    private static async Task<IResult> GetDatabaseHealth(
        NpgsqlDataSource dataSource,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var command = dataSource.CreateCommand(
                "SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = 'vector')");

            var result = await command.ExecuteScalarAsync(
                cancellationToken);

            var vectorEnabled = result is true;

            return vectorEnabled
                ? Results.Ok(new
                {
                    Database = "Connected",
                    VectorExtension = "Enabled"
                })
                : Results.Problem(
                    "PostgreSQL is connected, but pgvector is not enabled.",
                    statusCode: 503);
        }
        catch (NpgsqlException ex)
        {
            return Results.Problem(
                "PostgreSQL connection failed." + ex.Message,
                statusCode: 503);
        }
    }
}
