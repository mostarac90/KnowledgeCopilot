using KnowledgeCopilot.Core.Documents;
using KnowledgeCopilot.Core.Dtos;
using Npgsql;

namespace KnowledgeCopilot.Infrastructure.Documents
{
    public sealed class DocumentRepository(
      NpgsqlDataSource dataSource)
    {
        public async Task<Guid> SaveAsync(
        string fileName,
        byte[] pdfBytes,
        ExtractedDocument document,
        CancellationToken cancellationToken = default)
        {
            var documentId = Guid.NewGuid();

            await using var connection =
                await dataSource.OpenConnectionAsync(cancellationToken);

            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            // 1. Save document metadata and original PDF
            await using (var command = new NpgsqlCommand(
                """
            INSERT INTO documents
                (id, file_name, content_type,
                 size_bytes, original_pdf, page_count)
            VALUES
                (@id, @fileName, @contentType,
                 @sizeBytes, @pdfBytes, @pageCount)
            """, connection, transaction))
            {
                command.Parameters.AddWithValue("id", documentId);
                command.Parameters.AddWithValue("fileName", fileName);
                command.Parameters.AddWithValue(
                    "contentType", "application/pdf");
                command.Parameters.AddWithValue(
                    "sizeBytes", (long)pdfBytes.Length);
                command.Parameters.AddWithValue("pdfBytes", pdfBytes);
                command.Parameters.AddWithValue(
                    "pageCount", document.Pages.Count);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            // 2. Save text for each page
            foreach (var page in document.Pages)
            {
                await using var command = new NpgsqlCommand(
                    """
                INSERT INTO document_pages
                    (document_id, page_number, extracted_text)
                VALUES
                    (@documentId, @pageNumber, @text)
                """, connection, transaction);

                command.Parameters.AddWithValue(
                    "documentId", documentId);

                command.Parameters.AddWithValue(
                    "pageNumber", page.PageNumber);

                command.Parameters.AddWithValue(
                    "text", page.Text);

                await command.ExecuteNonQueryAsync(cancellationToken);
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
