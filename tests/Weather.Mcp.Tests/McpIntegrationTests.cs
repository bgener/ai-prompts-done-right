using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Weather.Mcp;

namespace Weather.Mcp.Tests;

/// <summary>
/// Starts a lightweight fake Weather API and the real MCP server in-process,
/// then exercises the MCP protocol over real HTTP. Port 0 lets the OS pick a
/// free port, so these tests are safe to run in parallel.
/// </summary>
public sealed class McpFixture : IAsyncLifetime
{
    private WebApplication? _fakeApi;
    private WebApplication? _mcp;
    public Uri McpEndpoint { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        _fakeApi = BuildFakeWeatherApi();
        _fakeApi.Urls.Clear();
        _fakeApi.Urls.Add("http://127.0.0.1:0");
        await _fakeApi.StartAsync();

        string apiUrl = _fakeApi.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();

        // Point the MCP server at the fake API via in-memory config override.
        IConfiguration extraConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["WeatherApi:BaseUrl"] = apiUrl })
            .Build();

        _mcp = McpHost.Build([], extraConfig);
        _mcp.Urls.Clear();
        _mcp.Urls.Add("http://127.0.0.1:0");
        await _mcp.StartAsync();

        string mcpUrl = _mcp.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First();
        McpEndpoint = new Uri(mcpUrl);
    }

    public async Task DisposeAsync()
    {
        if (_mcp is not null) await _mcp.DisposeAsync();
        if (_fakeApi is not null) await _fakeApi.DisposeAsync();
    }

    private static WebApplication BuildFakeWeatherApi()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder([]);
        WebApplication app = builder.Build();
        app.MapGet("/weatherforecast", (string city = "London", int days = 3) =>
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            return Enumerable.Range(0, Math.Clamp(days, 1, 10))
                .Select(i => new FakeForecast(city, today.AddDays(i), 15 + i, "Mild"));
        });
        return app;
    }

    private record FakeForecast(string City, DateOnly Date, int TemperatureC, string Summary);
}

[Trait("Category", "Integration")]
public sealed class McpIntegrationTests(McpFixture fixture) : IClassFixture<McpFixture>
{
    private Task<McpClient> ConnectAsync() =>
        McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = fixture.McpEndpoint,
            Name = "test"
        }));

    [Fact]
    public async Task Server_exposes_get_forecast_tool()
    {
        await using McpClient client = await ConnectAsync();
        IList<McpClientTool> tools = await client.ListToolsAsync();

        Assert.Contains(tools, t => t.Name == "get_forecast");
    }

    [Fact]
    public async Task Get_forecast_returns_structured_content_for_city()
    {
        await using McpClient client = await ConnectAsync();
        CallToolResult result = await client.CallToolAsync(
            "get_forecast",
            new Dictionary<string, object?> { ["city"] = "Amsterdam", ["days"] = 2 });

        Assert.NotEqual(true, result.IsError);
        Assert.NotNull(result.StructuredContent);
    }

    [Fact]
    public async Task Get_forecast_text_content_contains_city_name()
    {
        await using McpClient client = await ConnectAsync();
        CallToolResult result = await client.CallToolAsync(
            "get_forecast",
            new Dictionary<string, object?> { ["city"] = "Tokyo", ["days"] = 1 });

        string text = result.Content.OfType<TextContentBlock>().First().Text;
        Assert.Contains("Tokyo", text);
    }

    [Fact]
    public async Task Server_exposes_summarize_weather_prompt()
    {
        await using McpClient client = await ConnectAsync();
        IList<McpClientPrompt> prompts = await client.ListPromptsAsync();

        Assert.Contains(prompts, p => p.Name == "summarize_weather");
    }

    [Fact]
    public async Task Summarize_weather_prompt_renders_with_city_and_tone()
    {
        await using McpClient client = await ConnectAsync();
        GetPromptResult result = await client.GetPromptAsync(
            "summarize_weather",
            new Dictionary<string, object?> { ["city"] = "Amsterdam", ["tone"] = "formal" });

        Assert.NotEmpty(result.Messages);

        string allText = string.Join(" ", result.Messages
            .Select(m => m.Content)
            .OfType<TextContentBlock>()
            .Select(t => t.Text));

        Assert.Contains("Amsterdam", allText);
        Assert.Contains("formal", allText);
    }
}
