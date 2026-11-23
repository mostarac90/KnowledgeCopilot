using KnowledgeCopilot.Core.Documents;
using UglyToad.PdfPig;

namespace KnowledgeCopilot.Infrastructure.Documents;

public sealed class PdfTextExtractor : IDocumentTextExtractor
{
    public ExtractedDocument Extract(Stream documentStream)
    {
        using var pdfDocument = PdfDocument.Open(documentStream);
        var pages = new List<ExtractedPage>();
        
        for (int i = 1; i < pdfDocument.NumberOfPages; i++)
        {
            var text = pdfDocument.GetPage(i).Text;
            pages.Add(new ExtractedPage(i, text));
        }

        return new ExtractedDocument(pages);
    }
}
