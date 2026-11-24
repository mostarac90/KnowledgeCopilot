using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;

namespace KnowledgeCopilot.Infrastructure.Agents;

public sealed class KnowledgeCopilotAgent
{
    private readonly AIAgent _agent;

    public KnowledgeCopilotAgent(
        KnowledgeBaseTools knowledgeTools,
        string apiKey,
        string model)
    {
        var searchTool = AIFunctionFactory.Create(
            knowledgeTools.SearchKnowledgeBaseAsync);

        _agent = new OpenAIClient(apiKey)
            .GetChatClient(model)
            .AsAIAgent(
                name: "KnowledgeCopilot",
                instructions: """
                    You are an enterprise document assistant.

                    Your job is to answer questions using
                    information from the company's documents.

                    Rules:
                    1. Search the knowledge base when the user
                       asks about document contents.
                    2. Base document-related answers on retrieved
                       information.
                    3. If the documents do not contain enough
                       information, say so.
                    4. Never invent document facts.
                    5. Keep answers clear and concise.
                    6. Only answer using the information provided by the documents.
                    7. If the user asks for information not in the documents, politely inform them that you cannot provide an answer.
                    """,
                tools: [searchTool]);
    }

    public async Task<string> AskAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        var response = await _agent.RunAsync(
            question,
            cancellationToken: cancellationToken);

        return response.ToString();
    }
}