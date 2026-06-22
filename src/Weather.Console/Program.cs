using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

// Defaults connect to the MCP server started by docker compose.
string mcpUrl = args.ElementAtOrDefault(0) ?? "http://localhost:5151";
string city   = args.ElementAtOrDefault(1) ?? "Amsterdam";

Console.WriteLine($"MCP server : {mcpUrl}");
Console.WriteLine($"City       : {city}");
Console.WriteLine();

await using McpClient client = await McpClient.CreateAsync(
    new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri(mcpUrl),
        Name     = "weather-console"
    }));

//List tools
Console.WriteLine("[ tools/list ]");
IList<McpClientTool> tools = await client.ListToolsAsync();
foreach (McpClientTool tool in tools)
    Console.WriteLine($"  {tool.Name}  —  {tool.Description}");
Console.WriteLine();

// Call get_forecast
Console.WriteLine($"[ tools/call  get_forecast  city={city} days=3 ]");
CallToolResult forecast = await client.CallToolAsync(
    "get_forecast",
    new Dictionary<string, object?> { ["city"] = city, ["days"] = 3 });

if (forecast.IsError == true)
{
    Console.WriteLine("ERROR: " + string.Join(", ",
        forecast.Content.OfType<TextContentBlock>().Select(t => t.Text)));
}
else if (forecast.StructuredContent is not null)
{
    // UseStructuredContent=true on the tool: the SDK returns typed JSON.
    Console.WriteLine(JsonSerializer.Serialize(
        forecast.StructuredContent,
        new JsonSerializerOptions { WriteIndented = true }));
}
else
{
    foreach (TextContentBlock block in forecast.Content.OfType<TextContentBlock>())
        Console.WriteLine(block.Text);
}
Console.WriteLine();

// List prompts
Console.WriteLine("[ prompts/list ]");
IList<McpClientPrompt> prompts = await client.ListPromptsAsync();
foreach (McpClientPrompt prompt in prompts)
    Console.WriteLine($"  {prompt.Name}  —  {prompt.Description}");
Console.WriteLine();

// Render the summarize_weather prompt
Console.WriteLine($"[ prompts/get  summarize_weather  city={city} tone=casual ]");
GetPromptResult rendered = await client.GetPromptAsync(
    "summarize_weather",
    new Dictionary<string, object?> { ["city"] = city, ["tone"] = "casual", ["days"] = "3" });

foreach (PromptMessage message in rendered.Messages)
{
    Console.WriteLine($"  [{message.Role}]");
    if (message.Content is TextContentBlock text)
        Console.WriteLine($"  {text.Text}");
    Console.WriteLine();
}
