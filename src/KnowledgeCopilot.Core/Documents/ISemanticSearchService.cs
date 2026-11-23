namespace KnowledgeCopilot.Core.Documents;

public interface ISemanticSearchService
{
    Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(
        string query,
        int topK = 5,
        CancellationToken cancellationToken = default);
}