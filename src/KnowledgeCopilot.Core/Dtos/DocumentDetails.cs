using KnowledgeCopilot.Core.Documents;

namespace KnowledgeCopilot.Core.Dtos;

public sealed record DocumentDetails(
    Guid Id,
    string FileName,
    int PageCount,
    IReadOnlyList<ExtractedPage> Pages);
