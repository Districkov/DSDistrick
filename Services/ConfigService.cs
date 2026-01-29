using Microsoft.Extensions.Configuration;
using DiscordBot.Models;

namespace DiscordBot.Services
{
    public class ConfigService
    {
        private readonly BotConfig _config;

        public ConfigService(string configPath)
        {
            // Сначала пытаемся загрузить из переменных окружения
            var envToken = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: true, reloadOnChange: true)
                .Build();

            _config = new BotConfig
            {
                // Приоритет: переменная окружения > config.json > пусто
                BotToken = string.IsNullOrEmpty(envToken) 
                    ? (configuration["BotToken"] ?? "")
                    : envToken,
                ApplicationId = ulong.Parse(configuration["ApplicationId"] ?? "0"),
                FamilyApplicationsChannelId = ulong.Parse(configuration["FamilyApplicationsChannelId"] ?? "0"),
                TransportRequestChannelId = ulong.Parse(configuration["TransportRequestChannelId"] ?? "0"),
                ApplicationApprovalsChannelId = ulong.Parse(configuration["ApplicationApprovalsChannelId"] ?? "0"),
                ServerLogsChannelId = ulong.Parse(configuration["ServerLogsChannelId"] ?? "0"),
                SocialMediaChannelId = ulong.Parse(configuration["SocialMediaChannelId"] ?? "0"),
                TwitchChannelId = configuration["TwitchChannelId"],
                YouTubeChannelId = configuration["YouTubeChannelId"],
                TikTokUsername = configuration["TikTokUsername"]
            };
        }

        public BotConfig GetConfig() => _config;
    }
}
