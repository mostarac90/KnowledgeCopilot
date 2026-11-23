using KnowledgeCopilot.Core.Documents;
using KnowledgeCopilot.Core.Dtos;
using Npgsql;

namespace KnowledgeCopilot.Infrastructure.Documents
{
    public sealed class DocumentRepository(
      NpgsqlDataSource dataSource)
    {

        public async Task<Guid> SaveAsync(Stream documentStream, CancellationToken cancellationToken = default)
        {
            var documentId = Guid.NewGuid();
            var uploadedAt = DateTime.UtcNow;

            using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
            using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            // Extract text from the PDF document
            var textExtractor = new PdfTextExtractor();
            var extractedDocument = textExtractor.Extract(documentStream);


            // Insert the document metadata into the documents table
            var insertDocumentCommand = connection.CreateCommand();
            insertDocumentCommand.CommandText = @"
                INSERT INTO documents (document_id, uploaded_at)
                VALUES (@document_id, @uploaded_at)";
            insertDocumentCommand.Parameters.AddWithValue("document_id", documentId);
            insertDocumentCommand.Parameters.AddWithValue("uploaded_at", uploadedAt);
            await insertDocumentCommand.ExecuteNonQueryAsync(cancellationToken);

            // Insert each page into the document_pages table
            foreach (var page in extractedDocument.Pages)
            {
                var insertPageCommand = connection.CreateCommand();
                insertPageCommand.CommandText = @"
                    INSERT INTO document_pages (document_id, page_number, text)
                    VALUES (@document_id, @page_number, @text)";
                insertPageCommand.Parameters.AddWithValue("document_id", documentId);
                insertPageCommand.Parameters.AddWithValue("page_number", page.PageNumber);
                insertPageCommand.Parameters.AddWithValue("text", page.Text);

                await insertPageCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return documentId;
        }

        public async Task<List<DocumentSummary>> GetAllAsync(
         CancellationToken cancellationToken = default)
        {
            var documents = new List<DocumentSummary>();

            await using var command = dataSource.CreateCommand(
                """
            SELECT id, file_name, page_count, uploaded_at
            FROM documents
            ORDER BY uploaded_at DESC
            """);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                documents.Add(new DocumentSummary(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    reader.GetDateTime(3)));
            }

            return documents;
        }

        public async Task<DocumentDetails?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        {
            await using var command = dataSource.CreateCommand(
                """
            SELECT d.id, d.file_name, d.page_count,
                   p.page_number, p.extracted_text
            FROM documents d
            LEFT JOIN document_pages p ON p.document_id = d.id
            WHERE d.id = @id
            ORDER BY p.page_number
            """);

            command.Parameters.AddWithValue("id", id);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            string? fileName = null;
            var pageCount = 0;
            var pages = new List<ExtractedPage>();

            while (await reader.ReadAsync(cancellationToken))
            {
                fileName ??= reader.GetString(1);
                pageCount = reader.GetInt32(2);

                if (!reader.IsDBNull(3))
                {
                    pages.Add(new ExtractedPage(
                        reader.GetInt32(3),
                        reader.GetString(4)));
                }
            }

            return fileName is null
                ? null
                : new DocumentDetails(id, fileName, pageCount, pages);
        }
    }
}
