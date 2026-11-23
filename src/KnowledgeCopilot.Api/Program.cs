using Npgsql;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration
    .GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Postgres connection string is missing.");

builder.Services.AddSingleton(
    NpgsqlDataSource.Create(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Service = "KnowledgeCopilot.Api",
        Timestamp = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/health/database",
    async (NpgsqlDataSource dataSource,
           CancellationToken cancellationToken) =>
    {
        try
        {
            await using var command =
                dataSource.CreateCommand(
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
    });

app.Run();
