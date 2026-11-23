namespace KnowledgeCopilot.Core.RAG;

public sealed record RagResponse(
    string Question,
    string Answer,
    IReadOnlyList<RagSource> Sources);