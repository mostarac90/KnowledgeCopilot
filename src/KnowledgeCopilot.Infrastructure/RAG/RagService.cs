using global::KnowledgeCopilot.Core.Documents;
using global::KnowledgeCopilot.Core.RAG;
using System.Text;

namespace KnowledgeCopilot.Infrastructure.RAG;

public sealed class RagService
{
    private readonly ISemanticSearchService _searchService;
    private readonly IChatService _chatService;

    public RagService(
        ISemanticSearchService searchService,
        IChatService chatService)
    {
        _searchService = searchService;
        _chatService = chatService;
    }

    public async Task<RagResponse> AskAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException(
                "Question cannot be empty.",
                nameof(question));

        // 1. Retrieve the top 5 relevant chunks.
        var results = await _searchService.SearchAsync(
            question,
            topK: 5,
            cancellationToken: cancellationToken);

        // 2. No indexed documents or matches.
        if (results.Count == 0)
        {
            return new RagResponse(
                question,
                "I couldn't find that information " +
                "in the provided documents.",
                []);
        }

        // 3. Build the context provided to the LLM.
        var contextBuilder = new StringBuilder();
        var sources = new List<RagSource>();

        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var sourceId = i + 1;

            contextBuilder.AppendLine(
                $"[Source {sourceId}]");

            contextBuilder.AppendLine(
                $"Document: {result.FileName}");

            contextBuilder.AppendLine(
                $"Page: {result.PageNumber}");

            contextBuilder.AppendLine(
                $"Content: {result.Content}");

            contextBuilder.AppendLine();

            sources.Add(new RagSource(
                sourceId,
                result.DocumentId,
                result.FileName,
                result.PageNumber,
                result.ChunkIndex,
                result.Similarity));
        }

        // 4. Let the LLM generate an answer.
        var answer = await _chatService.GenerateAnswerAsync(
            question,
            contextBuilder.ToString(),
            cancellationToken);

        // 5. Return the answer and source metadata.
        return new RagResponse(
            question,
            answer,
            sources);
    }
}
