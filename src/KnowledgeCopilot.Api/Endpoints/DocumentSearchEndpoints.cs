namespace KnowledgeCopilot.Api.Endpoints;

public static class DocumentSearchEndpoints
{
    /// <summary>
    /// Registers every endpoint that belongs to document search:
    /// document upload/retrieval and semantic search.
    /// </summary>
    public static IEndpointRouteBuilder MapDocumentSearchEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapDocumentEndpoints();
        app.MapSearchEndpoints();

        return app;
    }
}
