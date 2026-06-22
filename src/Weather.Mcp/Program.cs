using Weather.Mcp;

McpHost.Build(args).Run();

// Exposed so WebApplicationFactory<Program> can find the entry point if integration tests are added.
public partial class Program;
