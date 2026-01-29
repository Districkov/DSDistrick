# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file
COPY ["DiscordBot.csproj", "./"]
RUN dotnet restore "DiscordBot.csproj"

# Copy source
COPY . .
RUN dotnet build "DiscordBot.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "DiscordBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=publish /app/publish .

# Copy config.json
COPY config.json .

# Run the bot
ENTRYPOINT ["dotnet", "DiscordBot.dll"]
