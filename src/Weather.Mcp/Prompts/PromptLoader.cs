using Prompty.Core;

namespace Weather.Mcp.Prompts;

public static class PromptLoader
{
    static PromptLoader()
    {
        // Register built-in .prompty parser and Mustache renderer.
        // PrepareAsync needs these; provider (OpenAI etc.) is only needed for Execute.
        InvokerRegistry.RegisterRenderer("jinja2", new Jinja2Renderer());
        InvokerRegistry.RegisterParser("prompty", new PromptyChatParser());
    }

    public static async Task<IReadOnlyList<(string Role, string Content)>> LoadAsync(
        string promptName, Dictionary<string, string> args)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Prompts", $"{promptName}.prompty");
        var spec = await PromptyLoader.LoadAsync(path, CancellationToken.None);
        var inputs = args.ToDictionary(kv => kv.Key, kv => (object?)kv.Value);
        var messages = await Pipeline.PrepareAsync(spec, inputs);
        return messages.Select(m => (m.Role.ToString().ToLowerInvariant(), m.Text)).ToList();
    }
}
