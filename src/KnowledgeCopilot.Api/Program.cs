using KnowledgeCopilot.Api.Endpoints;
using KnowledgeCopilot.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPostgres(builder.Configuration)
    .AddDocumentServices()
    .AddEmbeddingServices()
    .AddRagServices()
    .AddAgentServices()
    .AddSwaggerDocumentation();

var app = builder.Build();

app.UseSwaggerDocumentation();

app.MapHealthEndpoints();
app.MapDocumentSearchEndpoints();
app.MapAskEndpoints();
app.MapAgentEndpoints();

app.Run();
