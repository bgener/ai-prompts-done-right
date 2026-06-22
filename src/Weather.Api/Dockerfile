FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["global.json", "."]
COPY ["src/Weather.Api/Weather.Api.csproj", "src/Weather.Api/"]
RUN dotnet restore "src/Weather.Api/Weather.Api.csproj"
COPY . .
WORKDIR "/src/src/Weather.Api"
RUN dotnet publish "Weather.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Weather.Api.dll"]
