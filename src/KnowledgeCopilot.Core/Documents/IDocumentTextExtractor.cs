namespace KnowledgeCopilot.Core.Documents;

public interface IDocumentTextExtractor
{
    ExtractedDocument Extract(Stream documentStream);
}