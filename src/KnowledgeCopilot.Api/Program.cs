using KnowledgeCopilot.Api.Endpoints;
using KnowledgeCopilot.Api.Extensions;
using KnowledgeCopilot.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgres(builder.Configuration)
    .AddDocumentServices()
    .AddEmbeddingServices()
    .AddRagServices()
    .AddAgentServices()
    .AddSwaggerDocumentation();


// Add any origin for now, but we should restrict this in production to only allow the frontend origin.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Create/upgrade the database schema before handling any requests.
await app.Services.GetRequiredService<DatabaseMigrator>().MigrateAsync();

app.UseSwaggerDocumentation();

app.UseCors();

app.MapHealthEndpoints();
app.MapDocumentSearchEndpoints();
app.MapAskEndpoints();
app.MapAgentEndpoints();



app.Run();
