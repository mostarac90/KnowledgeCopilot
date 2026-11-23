namespace KnowledgeCopilot.Core.Dtos;

public sealed record DocumentSummary(
    Guid Id,
    string FileName,
    int PageCount,
    DateTime UploadedAt);
