using Weather.Mcp.Prompts;

namespace Weather.Mcp.Tests;

public sealed class PromptLoaderTests
{
    private static async Task<IReadOnlyList<(string Role, string Content)>> FromFile(
        string city, string tone, string days = "3") =>
        await PromptLoader.LoadAsync("summarize-weather", new Dictionary<string, string>
        {
            ["city"] = city,
            ["tone"] = tone,
            ["days"] = days
        });

    [Fact]
    public async Task File_loads_without_error()
    {
        var messages = await FromFile("Amsterdam", "casual");
        Assert.NotEmpty(messages);
    }

    [Fact]
    public async Task File_has_system_and_user_messages()
    {
        var messages = await FromFile("Amsterdam", "casual");
        Assert.Contains(messages, m => m.Role == "system");
        Assert.Contains(messages, m => m.Role == "user");
    }

    [Theory]
    [InlineData("Amsterdam")]
    [InlineData("Tokyo")]
    [InlineData("Oslo")]
    public async Task City_is_in_user_message(string city)
    {
        string user = (await FromFile(city, "casual")).First(m => m.Role == "user").Content;
        Assert.Contains(city, user);
    }

    [Theory]
    [InlineData("casual")]
    [InlineData("formal")]
    [InlineData("technical")]
    public async Task Tone_is_in_system_message(string tone)
    {
        string system = (await FromFile("London", tone)).First(m => m.Role == "system").Content;
        Assert.Contains(tone, system);
    }

    [Fact]
    public async Task Days_is_in_user_message()
    {
        string user = (await FromFile("Berlin", "casual", "7")).First(m => m.Role == "user").Content;
        Assert.Contains("7", user);
    }

    [Fact]
    public async Task No_unreplaced_placeholders_in_file()
    {
        var messages = await FromFile("Oslo", "formal", "5");
        foreach ((_, string content) in messages)
            Assert.DoesNotContain("{{", content);
    }
}
