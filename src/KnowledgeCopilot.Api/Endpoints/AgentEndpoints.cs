using KnowledgeCopilot.Api.Dtos;
using KnowledgeCopilot.Infrastructure.Agents;

namespace KnowledgeCopilot.Api.Endpoints;

public static class AgentEndpoints
{
    private const int MaxQuestionLength = 2000;

    public static IEndpointRouteBuilder MapAgentEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/agent/ask", Ask);

        return app;
    }

    private static async Task<IResult> Ask(
        AskRequest request,
        KnowledgeCopilotAgent agent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return Results.BadRequest(new
            {
                Error = "Question cannot be empty."
            });
        }

        if (request.Question.Length > MaxQuestionLength)
        {
            return Results.BadRequest(new
            {
                Error = "Question is too long."
            });
        }

        var answer = await agent.AskAsync(
            request.Question,
            cancellationToken);

        return Results.Ok(new
        {
            request.Question,
            Answer = answer
        });
    }
}
