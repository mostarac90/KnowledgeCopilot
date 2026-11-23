namespace KnowledgeCopilot.Core.Document;

public sealed record DocumentChunk(
    int PageNumber,
    int ChunkIndex,
    string Content);