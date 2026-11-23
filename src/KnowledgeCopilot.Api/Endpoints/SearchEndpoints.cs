using KnowledgeCopilot.Core.Documents;

namespace KnowledgeCopilot.Api.Endpoints;

public static class SearchEndpoints
{
    private const int MaxQueryLength = 2000;
    private const int MinTopK = 1;
    private const int MaxTopK = 20;

    public static IEndpointRouteBuilder MapSearchEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search", Search);

        return app;
    }

    private static async Task<IResult> Search(
        ISemanticSearchService searchService,
        string query,
        CancellationToken cancellationToken,
        int topK = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Results.BadRequest("Query cannot be empty.");

        if (query.Length > MaxQueryLength)
            return Results.BadRequest(
                $"Query cannot exceed {MaxQueryLength} characters.");

        if (topK < MinTopK || topK > MaxTopK)
            return Results.BadRequest(
                $"topK must be between {MinTopK} and {MaxTopK}.");

        var results = await searchService.SearchAsync(
            query,
            topK,
            cancellationToken);

        return Results.Ok(new
        {
            Query = query,
            TotalResults = results.Count,
            Results = results
        });
    }
}
