using KnowledgeCopilot.Core.Document;
using KnowledgeCopilot.Core.Documents;
using Npgsql;
using Pgvector;

namespace KnowledgeCopilot.Infrastructure.Documents;

public sealed class DocumentChunkRepository(NpgsqlDataSource dataSource)
{

    public async Task SaveAsync(
    Guid documentId,
    IReadOnlyList<(DocumentChunk Chunk, float[] Embedding)> items,
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        // Remove previously indexed chunks.
        await using (var delete = new NpgsqlCommand(
            "DELETE FROM document_chunks WHERE document_id = @documentId",
            connection,
            transaction))
        {
            delete.Parameters.AddWithValue("documentId", documentId);

            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        const string sql = """
        INSERT INTO document_chunks
            (id, document_id, page_number,
             chunk_index, content, embedding, embedding_model)
        VALUES
            (@id, @documentId, @pageNumber,
             @chunkIndex, @content, @embedding, @model)
        """;

        foreach (var item in items)
        {
            await using var command =
                new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.AddWithValue(
                "id", Guid.NewGuid());

            command.Parameters.AddWithValue(
                "documentId", documentId);

            command.Parameters.AddWithValue(
                "pageNumber", item.Chunk.PageNumber);

            command.Parameters.AddWithValue(
                "chunkIndex", item.Chunk.ChunkIndex);

            command.Parameters.AddWithValue(
                "content", item.Chunk.Content);

            command.Parameters.AddWithValue(
                "embedding", new Vector(item.Embedding));

            command.Parameters.AddWithValue(
                "model", "text-embedding-3-small");

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<DocumentSearchResult>> SearchAsync(
        float[] queryEmbedding,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (queryEmbedding.Length != 1536)
            throw new ArgumentException(
                "Expected a 1536-dimensional embedding.",
                nameof(queryEmbedding));

        if (topK is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(topK));

        const string sql = """
            SELECT
                dc.document_id,
                d.file_name,
                dc.page_number,
                dc.chunk_index,
                dc.content,
                1 - (dc.embedding <=> @embedding) AS similarity
            FROM document_chunks dc
            INNER JOIN documents d
                ON dc.document_id = d.id
            WHERE dc.embedding_model = @model
            ORDER BY dc.embedding <=> @embedding
            LIMIT @topK;
            """;

        var results = new List<DocumentSearchResult>();

        await using var command = dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "embedding", new Vector(queryEmbedding));

        command.Parameters.AddWithValue(
            "model", "text-embedding-3-small");

        command.Parameters.AddWithValue("topK", topK);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new DocumentSearchResult(
                DocumentId: reader.GetGuid(0),
                FileName: reader.GetString(1),
                PageNumber: reader.GetInt32(2),
                ChunkIndex: reader.GetInt32(3),
                Content: reader.GetString(4),
                Similarity: reader.GetDouble(5)));
        }

        return results;
    }
}
