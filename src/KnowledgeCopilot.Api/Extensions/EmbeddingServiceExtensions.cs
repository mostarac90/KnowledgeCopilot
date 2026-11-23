using KnowledgeCopilot.Core.Documents;
using KnowledgeCopilot.Infrastructure.Embeddings;

namespace KnowledgeCopilot.Api.Extensions;

public static class EmbeddingServiceExtensions
{
    public static IServiceCollection AddEmbeddingServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IEmbeddingService>(sp =>
        {
            var configuration =
                sp.GetRequiredService<IConfiguration>();

            var apiKey = configuration["OPENAI_API_KEY"]
                ?? throw new InvalidOperationException(
                    "OpenAI API key is missing.");

            return new OpenAIEmbeddingService(apiKey);
        });

        return services;
    }
}
