using KnowledgeCopilot.Core.Documents;
using KnowledgeCopilot.Infrastructure.Documents;

namespace KnowledgeCopilot.Api.Endpoints;

public static class DocumentEndpoints
{
    // We set the max file size to 10 MB
    private const long MaxFileSize = 10 * 1024 * 1024;

    public static IEndpointRouteBuilder MapDocumentEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents");

        group.MapPost("/", UploadDocument)
            .DisableAntiforgery();

        group.MapGet("/", GetAllDocuments);
        group.MapGet("/{id:guid}", GetDocumentById);

        return app;
    }

    private static async Task<IResult> UploadDocument(
        IFormFile file,
        IDocumentTextExtractor extractor,
        DocumentRepository repository,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return Results.BadRequest("File is empty.");

        if (file.Length > MaxFileSize)
            return Results.BadRequest(
                "Maximum file size is 10 MB.");

        // We only support PDF files for now
        if (!Path.GetExtension(file.FileName)
            .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(
                "Only PDF files are supported.");
        }

        await using var stream = new MemoryStream();

        await file.CopyToAsync(stream, cancellationToken);

        var pdfBytes = stream.ToArray();

        // Basic PDF signature validation
        if (pdfBytes.Length < 5 ||
            !pdfBytes.AsSpan(0, 5).SequenceEqual("%PDF-"u8))
        {
            return Results.BadRequest(
                "The uploaded file is not a valid PDF.");
        }

        stream.Position = 0;

        var extractedDocument = extractor.Extract(stream);

        if (extractedDocument.Pages.All(
            p => string.IsNullOrWhiteSpace(p.Text)))
        {
            return Results.BadRequest(
                "No extractable text found. Scanned PDFs require OCR.");
        }

        var documentId = await repository.SaveAsync(
            file.FileName,
            pdfBytes,
            extractedDocument,
            cancellationToken);

        return Results.Created(
            $"/api/documents/{documentId}",
            new
            {
                Id = documentId,
                FileName = file.FileName,
                PageCount = extractedDocument.Pages.Count,
                Status = "Processed"
            });
    }

    private static async Task<IResult> GetAllDocuments(
        DocumentRepository repository,
        CancellationToken cancellationToken)
    {
        var documents = await repository.GetAllAsync(
            cancellationToken);

        return Results.Ok(documents);
    }

    private static async Task<IResult> GetDocumentById(
        Guid id,
        DocumentRepository repository,
        CancellationToken cancellationToken)
    {
        var document = await repository.GetByIdAsync(
            id, cancellationToken);

        return document is null
            ? Results.NotFound()
            : Results.Ok(document);
    }
}
