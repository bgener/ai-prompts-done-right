using Microsoft.Extensions.Configuration;

namespace Weather.Mcp;

public static class McpHost
{
    public static WebApplication Build(string[] args, IConfiguration? extraConfig = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // extraConfig lets tests inject a base URL for the fake Weather API without
        // touching environment variables or appsettings files.
        if (extraConfig is not null)
            builder.Configuration.AddConfiguration(extraConfig);

        builder.Services
            .AddHttpClient<WeatherApiClient>(client =>
                client.BaseAddress = new Uri(
                    builder.Configuration["WeatherApi:BaseUrl"] ?? "http://localhost:5150"));

        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly()
            .WithPromptsFromAssembly();

        WebApplication app = builder.Build();
        app.MapMcp();
        return app;
    }
}
