using KnowledgeCopilot.Core.Documents;

namespace KnowledgeCopilot.Infrastructure.Documents;

public sealed class SemanticSearchService(
    IEmbeddingService embeddingService,
    DocumentChunkRepository repository)
    : ISemanticSearchService
{
    public async Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException(
                "Search query cannot be empty.",
                nameof(query));

        if (topK is < 1 or > 20)
            throw new ArgumentOutOfRangeException(
                nameof(topK));

        // Step 1: Convert the question to an embedding.
        var embedding =
            await embeddingService.GenerateEmbeddingAsync(
                query,
                cancellationToken);

        // Step 2: Find the closest stored embeddings.
        return await repository.SearchAsync(
            embedding,
            topK,
            cancellationToken);
    }
}
