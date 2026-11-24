using System.ComponentModel;
using KnowledgeCopilot.Core.Documents;

namespace KnowledgeCopilot.Infrastructure.Agents;

public sealed class KnowledgeBaseTools
{
    private readonly ISemanticSearchService _searchService;

    public KnowledgeBaseTools(
        ISemanticSearchService searchService)
    {
        _searchService = searchService;
    }

    [Description(
        "Search indexed documents for information relevant " +
        "to the user's question.")]
    public async Task<string> SearchKnowledgeBaseAsync(
        [Description("The question or search query")]
        string query,
        CancellationToken cancellationToken = default)
    {
        // Call the semantic search service to find relevant documents based on the query
        var results = await _searchService.SearchAsync(
            query,
            5,
            cancellationToken);

        if (!results.Any())
            return "No relevant documents found.";

        return System.Text.Json.JsonSerializer.Serialize(results);
    }
}