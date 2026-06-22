var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (string city = "London", int days = 5) =>
{
    days = Math.Clamp(days, 1, 10);

    // Deterministic seed so the same city always returns consistent data across calls.
    var seed = city.Trim().ToLowerInvariant().Sum(c => (int)c);
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    return Enumerable.Range(0, days).Select(i => new WeatherForecast(
        city,
        today.AddDays(i),
        ((seed + i * 7) % 43) - 5,
        summaries[(seed + i) % summaries.Length]));
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(string City, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
