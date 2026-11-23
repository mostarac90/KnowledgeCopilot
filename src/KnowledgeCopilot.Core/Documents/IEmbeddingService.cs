namespace KnowledgeCopilot.Core.Documents;

public interface IEmbeddingService
{
      Task<float[]> GenerateEmbeddingAsync(
         string text,
         CancellationToken cancellationToken = default);
}