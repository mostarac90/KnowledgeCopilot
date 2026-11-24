using KnowledgeCopilot.Infrastructure.Agents;

namespace KnowledgeCopilot.Api.Extensions;

public static class AgentServiceExtensions
{
    private const string DefaultChatModel = "gpt-4o-mini";

    public static IServiceCollection AddAgentServices(
        this IServiceCollection services)
    {
        // Scoped: the tools depend on ISemanticSearchService,
        // which is scoped (it uses the DB-backed repositories).
        services.AddScoped<KnowledgeBaseTools>();

        services.AddScoped<KnowledgeCopilotAgent>(sp =>
        {
            var configuration =
                sp.GetRequiredService<IConfiguration>();

            var apiKey = configuration["OPENAI_API_KEY"]
                ?? configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException(
                    "OpenAI API key is missing.");

            var model = configuration["OpenAI:ChatModel"]
                ?? DefaultChatModel;

            return new KnowledgeCopilotAgent(
                sp.GetRequiredService<KnowledgeBaseTools>(),
                apiKey,
                model);
        });

        return services;
    }
}
