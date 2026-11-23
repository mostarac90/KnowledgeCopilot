using KnowledgeCopilot.Core.Document;
using KnowledgeCopilot.Core.Documents;

namespace KnowledgeCopilot.Infrastructure.Documents;

public sealed class TextChunker
{
    private const int ChunkSize = 1000;
    private const int ChunkOverlap = 200;

    public IReadOnlyList<DocumentChunk> Chunk(
        IReadOnlyList<ExtractedPage> pages)
    {
        var chunks = new List<DocumentChunk>();

        foreach (var page in pages)
        {
            var text = page.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                continue;

            var chunkIndex = 0;
            var start = 0;

            while (start < text.Length)
            {
                var length = Math.Min(
                    ChunkSize,
                    text.Length - start);

                var content = text
                    .Substring(start, length)
                    .Trim();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    chunks.Add(new DocumentChunk(
                        page.PageNumber,
                        chunkIndex++,
                        content));
                }

                if (start + length >= text.Length)
                    break;

                start += ChunkSize - ChunkOverlap;
            }
        }

        return chunks;
    }
}