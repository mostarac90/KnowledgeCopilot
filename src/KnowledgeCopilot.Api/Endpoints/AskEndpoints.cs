using KnowledgeCopilot.Api.Dtos;
using KnowledgeCopilot.Infrastructure.RAG;

namespace KnowledgeCopilot.Api.Endpoints;

public static class AskEndpoints
{
    private const int MaxQuestionLength = 2000;

    public static IEndpointRouteBuilder MapAskEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ask", Ask);

        return app;
    }

    private static async Task<IResult> Ask(
        AskRequest request,
        RagService ragService,
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

        var response = await ragService.AskAsync(
            request.Question,
            cancellationToken);

        return Results.Ok(response);
    }
}
