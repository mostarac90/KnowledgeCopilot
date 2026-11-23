namespace KnowledgeCopilot.Core.Documents;

public sealed record DocumentSearchResult(
    Guid DocumentId,
    string FileName,
    int PageNumber,
    int ChunkIndex,
    string Content,
    double Similarity);