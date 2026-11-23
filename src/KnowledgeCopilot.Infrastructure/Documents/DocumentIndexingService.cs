using global::KnowledgeCopilot.Core.Document;
using global::KnowledgeCopilot.Core.Documents;

namespace KnowledgeCopilot.Infrastructure.Documents;

public sealed class DocumentIndexingService
{
    private readonly DocumentRepository _documents;
    private readonly DocumentChunkRepository _chunks;
    private readonly TextChunker _chunker;
    private readonly IEmbeddingService _embeddings;

    public DocumentIndexingService(
        DocumentRepository documents,
        DocumentChunkRepository chunks,
        TextChunker chunker,
        IEmbeddingService embeddings)
    {
        _documents = documents;
        _chunks = chunks;
        _chunker = chunker;
        _embeddings = embeddings;
    }

    public async Task<int?> IndexAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve the document and its extracted pages.
        var document = await _documents.GetByIdAsync(
            documentId,
            cancellationToken);

        if (document is null)
            return null;

        // 2. Split the extracted text into chunks.
        var chunks = _chunker.Chunk(document.Pages);

        if (chunks.Count == 0)
        {
            throw new InvalidOperationException(
                "Document contains no indexable text.");
        }

        var indexedChunks =
            new List<(DocumentChunk Chunk, float[] Embedding)>();

        // 3. Generate embeddings for every chunk.
        foreach (var chunk in chunks)
        {
            var embedding =
                await _embeddings.GenerateEmbeddingAsync(
                    chunk.Content,
                    cancellationToken);

            if (embedding.Length != 1536)
            {
                throw new InvalidOperationException(
                    "Unexpected embedding dimensions.");
            }

            indexedChunks.Add((chunk, embedding));
        }

        // 4. Persist the chunks and their embeddings.
        await _chunks.SaveAsync(
            documentId,
            indexedChunks,
            cancellationToken);

        return indexedChunks.Count;
    }
}
