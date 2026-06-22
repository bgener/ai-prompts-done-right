using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Weather.Mcp.Prompts;

[McpServerPromptType]
public sealed class ForecastPrompts
{
    [McpServerPrompt(Name = "summarize_weather")]
    [Description("Summarize a city's weather forecast in a chosen tone.")]
    public static async Task<IEnumerable<ChatMessage>> SummarizeAsync(
        [Description("City name, for example Amsterdam")] string city,
        [Description("Tone: casual, formal, or technical")] string tone = "casual",
        [Description("Number of days to summarize, for example 3")] string days = "3")
    {
        var messages = await PromptLoader.LoadAsync(
            "summarize-weather",
            new Dictionary<string, string>
            {
                ["city"] = city,
                ["tone"] = tone,
                ["days"] = days
            });

        return messages.Select(m => new ChatMessage(
            m.Role switch
            {
                "system"    => ChatRole.System,
                "assistant" => ChatRole.Assistant,
                _           => ChatRole.User
            },
            m.Content));
    }
}
