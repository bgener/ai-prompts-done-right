using System.Net.Http.Json;

namespace Weather.Mcp;

public record WeatherForecast(string City, DateOnly Date, int TemperatureC, string Summary)
{
    public int TemperatureF => 32 + (int)Math.Round(TemperatureC * 9.0 / 5.0);
}

public sealed class WeatherApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<WeatherForecast>> GetForecastAsync(
        string city, int days, CancellationToken ct) =>
        await http.GetFromJsonAsync<IReadOnlyList<WeatherForecast>>(
            $"/weatherforecast?city={Uri.EscapeDataString(city)}&days={days}", ct) ?? [];
}
