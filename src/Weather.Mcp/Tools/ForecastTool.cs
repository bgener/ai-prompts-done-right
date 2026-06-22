using System.ComponentModel;
using ModelContextProtocol.Server;
using Weather.Mcp;

namespace Weather.Mcp.Tools;

[McpServerToolType]
public sealed class ForecastTool
{
    [McpServerTool(Name = "get_forecast", Title = "Get weather forecast", ReadOnly = true, OpenWorld = true, UseStructuredContent = true)]
    [Description("Get a multi-day weather forecast for a city.")]
    public static async Task<IReadOnlyList<WeatherForecast>> GetForecast(
        WeatherApiClient weather,
        [Description("City name, for example Amsterdam")] string city,
        [Description("Number of forecast days, 1 to 10")] int days = 3,
        CancellationToken ct = default)
    {
        days = Math.Clamp(days, 1, 10);
        return await weather.GetForecastAsync(city, days, ct);
    }
}
