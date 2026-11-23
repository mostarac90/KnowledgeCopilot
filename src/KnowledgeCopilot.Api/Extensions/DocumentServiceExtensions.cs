using KnowledgeCopilot.Core.Documents;
using KnowledgeCopilot.Infrastructure.Documents;

namespace KnowledgeCopilot.Api.Extensions;

public static class DocumentServiceExtensions
{
    public static IServiceCollection AddDocumentServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IDocumentTextExtractor, PdfTextExtractor>();
        services.AddSingleton<TextChunker>();

        services.AddScoped<DocumentRepository>();
        services.AddScoped<DocumentChunkRepository>();

        services.AddScoped<
            ISemanticSearchService,
            SemanticSearchService>();

        return services;
    }
}
