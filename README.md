# Prompts done right — MCP server demo

Two separate services. One MCP server. One prompt file that is the single source of truth for runtime, unit tests, and LLM quality evals.

```
Weather.Api   — standard dotnet minimal API, no AI awareness
Weather.Mcp   — MCP server: one tool, one .prompty file
Weather.Console — console client that calls the MCP server
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for `docker compose`)
- [Node.js 18+](https://nodejs.org/) (for promptfoo eval only)

## Run with Docker Compose

```bash
docker compose up
```

This starts both services:

| Service | Host port | URL |
|---------|-----------|-----|
| Weather.Api | 5150 | `http://localhost:5150/weatherforecast?city=Amsterdam&days=3` |
| Weather.Mcp | 5151 | `http://localhost:5151` (connect your MCP client here) |

The MCP server learns the Weather.Api address from the `WeatherApi__BaseUrl` environment variable set in `docker-compose.yml`. In development (`dotnet run`) it reads the same key from `appsettings.json`.

## Run the console client

Start Docker Compose first, then:

```bash
dotnet run --project src/Weather.Console
# or with a different city
dotnet run --project src/Weather.Console -- http://localhost:5151 Tokyo
```

The console lists tools, calls `get_forecast`, lists prompts, and renders the `summarize_weather` prompt.

## Run without Docker (two terminals)

```bash
# terminal 1
dotnet run --project src/Weather.Api

# terminal 2
dotnet run --project src/Weather.Mcp
```

Weather.Api listens on `http://localhost:5150`. Weather.Mcp is pre-configured to call that address.

## Run tests

```bash
dotnet test PromptsDemo.slnx
```

19 tests total:

- **14 unit tests** — verify the `.prompty` template renders correctly (no LLM, no network, runs in under 100ms)
- **5 integration tests** — start both servers in-process on random ports, call `tools/list`, `tools/call`, `prompts/list`, and `prompts/get` over real HTTP

## Run promptfoo eval

Promptfoo evaluates the actual LLM output quality. It reads the real `.prompty` file, not a copy.

```bash
cd eval
npm install

# set one API key
export OPENAI_API_KEY=sk-...
# or
export ANTHROPIC_API_KEY=sk-ant-...

npm run eval
```

To open the results in a browser:

```bash
npm run view
```

### What the eval checks

Each test case runs the prompt against the model and asserts:

- No unreplaced `{{placeholders}}` in the output (applied to every case)
- City name appears in the response
- The chosen tone is actually reflected in the language (`llm-rubric`)
- Word count stays under 100

### Experimenting with the prompt

Edit `src/Weather.Mcp/Prompts/summarize-weather.prompty`, then re-run the eval. The eval always reads the file you just edited.

`promptfoo.yaml` already has a second entry labelled `experiment` so you can compare the current prompt against a draft side by side:

```
npm run eval    # shows current vs experiment columns
npm run view    # browser UI with diff view
```

### Prompt versioning

The `.prompty` frontmatter has a `version` field. Bump it when the prompt changes in a meaningful way. Git history is the full record. The promptfoo scores are the quality gate before you commit a prompt change.

## Project structure

```
demo-prompts/
  docker-compose.yml
  src/
    Weather.Api/           minimal weather API (no AI)
    Weather.Mcp/
      Tools/ForecastTool.cs
      Prompts/
        summarize-weather.prompty   single source of truth
        PromptLoader.cs
        ForecastPrompts.cs
    Weather.Console/       MCP client demo
  tests/
    Weather.Mcp.Tests/     unit + integration tests
  eval/
    promptfoo.yaml         LLM quality eval
    package.json
```
