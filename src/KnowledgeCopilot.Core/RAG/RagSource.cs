namespace KnowledgeCopilot.Core.RAG;

public sealed record RagSource(
    int SourceId,
    Guid DocumentId,
    string FileName,
    int PageNumber,
    int ChunkIndex,
    double Similarity);