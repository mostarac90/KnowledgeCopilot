namespace KnowledgeCopilot.Core.RAG;

public interface IChatService
{
    Task<string> GenerateAnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken = default);
}
