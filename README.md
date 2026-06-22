# AI Prompts Done Right

Demo for the article [AI Prompts Done Right: Use .prompty Files in .NET](https://bgener.nl).

One `.prompty` file is the source of truth for the MCP server prompt, unit tests, and LLM evals. No hardcoded strings.

## Run

```powershell
pwsh -File run-demo.ps1
```

Starts Weather.Api and Weather.Mcp, waits for both, runs the console client, then stops everything.

## Test

```bash
dotnet test PromptsDemo.slnx
```

## Connect Copilot CLI

```bash
dotnet run --project src/Weather.Mcp --urls http://localhost:5151

copilot mcp add --transport http weather http://localhost:5151
copilot -p "Get the 5-day forecast for Amsterdam in a formal tone."
```