namespace KnowledgeCopilot.Core.Documents;

public sealed record ExtractedDocument(
    IReadOnlyList<ExtractedPage> Pages);