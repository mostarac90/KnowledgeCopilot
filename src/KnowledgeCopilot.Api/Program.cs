using Npgsql;
using KnowledgeCopilot.Core.Documents;
using KnowledgeCopilot.Infrastructure.Documents;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration
    .GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Postgres connection string is missing.");

builder.Services.AddSingleton(
    NpgsqlDataSource.Create(connectionString));

builder.Services.AddSingleton<
    IDocumentTextExtractor,
    PdfTextExtractor>();

builder.Services.AddScoped<DocumentRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health check endpoints

app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Service = "KnowledgeCopilot.Api",
        Timestamp = DateTimeOffset.UtcNow
    });
});

app.MapGet("/api/health/database",
    async (NpgsqlDataSource dataSource,
           CancellationToken cancellationToken) =>
    {
        try
        {
            await using var command =
                dataSource.CreateCommand(
                    "SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname = 'vector')");

            var result = await command.ExecuteScalarAsync(
                cancellationToken);

            var vectorEnabled = result is true;

            return vectorEnabled
                ? Results.Ok(new
                {
                    Database = "Connected",
                    VectorExtension = "Enabled"
                })
                : Results.Problem(
                    "PostgreSQL is connected, but pgvector is not enabled.",
                    statusCode: 503);
        }
        catch (NpgsqlException ex)
        {
            return Results.Problem(
                "PostgreSQL connection failed." + ex.Message,
                statusCode: 503);
        }
    });


// Document management endpoints

app.MapPatch("/api/documents",
     async (
        IFormFile file,
        IDocumentTextExtractor extractor,
        DocumentRepository repository,
        CancellationToken cancellationToken) =>
     {

         // We set the max file size to 10 MB
         const long maxFileSize = 10 * 1024 * 1024;

         if (file.Length == 0)
             return Results.BadRequest("File is empty.");

         if (file.Length > maxFileSize)
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

         await file.CopyToAsync(
             stream, cancellationToken);

         var pdfBytes = stream.ToArray();

         // Basic PDF signature validation
         if (pdfBytes.Length < 5 ||
             !pdfBytes.AsSpan(0, 5)
                 .SequenceEqual("%PDF-"u8))
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
     })
    .DisableAntiforgery();


app.MapGet("/api/documents",
    async (DocumentRepository repository,
           CancellationToken cancellationToken) =>
    {
        var documents = await repository.GetAllAsync(
            cancellationToken);

        return Results.Ok(documents);
    });

app.Run();
