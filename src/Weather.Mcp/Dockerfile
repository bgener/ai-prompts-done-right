FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["global.json", "."]
COPY ["src/Weather.Mcp/Weather.Mcp.csproj", "src/Weather.Mcp/"]
RUN dotnet restore "src/Weather.Mcp/Weather.Mcp.csproj"
COPY . .
WORKDIR "/src/src/Weather.Mcp"
RUN dotnet publish "Weather.Mcp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Weather.Mcp.dll"]
