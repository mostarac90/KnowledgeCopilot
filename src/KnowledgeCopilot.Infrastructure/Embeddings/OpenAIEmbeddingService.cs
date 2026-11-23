    using KnowledgeCopilot.Core.Documents;
    using OpenAI.Embeddings;

    namespace KnowledgeCopilot.Infrastructure.Embeddings;

    public sealed class OpenAIEmbeddingService
        : IEmbeddingService
    {
        private readonly EmbeddingClient _client;

        public OpenAIEmbeddingService(string apiKey)
        {
            _client = new EmbeddingClient(
                "text-embedding-3-small",
                apiKey);
        }

        public async Task<float[]> GenerateEmbeddingAsync(
                string text,
                CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException(
                    "Text cannot be empty.", nameof(text));

            var result = await _client.GenerateEmbeddingAsync(
                        text,
                        cancellationToken: cancellationToken);

            return result.Value.ToFloats().ToArray();
        }
    }
