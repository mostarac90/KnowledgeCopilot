using KnowledgeCopilot.Core.RAG;
using KnowledgeCopilot.Infrastructure.RAG;

namespace KnowledgeCopilot.Api.Extensions;

public static class RagServiceExtensions
{
    private const string DefaultChatModel = "gpt-4o-mini";

    public static IServiceCollection AddRagServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IChatService>(sp =>
        {
            var configuration =
                sp.GetRequiredService<IConfiguration>();

            // Same key as the embedding service, with the
            // structured "OpenAI:ApiKey" setting as an alternative.
            var apiKey = configuration["OPENAI_API_KEY"]
                ?? configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException(
                    "OpenAI API key is missing.");

            var model = configuration["OpenAI:ChatModel"]
                ?? DefaultChatModel;

            return new OpenAIChatService(apiKey, model);
        });

        services.AddScoped<RagService>();

        return services;
    }
}
