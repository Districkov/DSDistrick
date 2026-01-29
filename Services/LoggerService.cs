using Discord;
using DiscordBot.Models;

namespace DiscordBot.Services
{
    public class LoggerService
    {
        private List<ServerLog> _logs = new();
        private const int MaxLogs = 1000;
        private IMessageChannel _logChannel;

        public void SetLogChannel(IMessageChannel channel)
        {
            _logChannel = channel;
        }

        public async Task LogEventAsync(string eventType, string description, ulong? userId = null, string userName = null)
        {
            var log = new ServerLog
            {
                Timestamp = DateTime.Now,
                EventType = eventType,
                Description = description,
                UserId = userId,
                UserName = userName ?? "Unknown"
            };

            _logs.Add(log);

            // Если логов больше чем MaxLogs, удаляем самые старые
            if (_logs.Count > MaxLogs)
            {
                _logs = _logs.Skip(_logs.Count - MaxLogs).ToList();
            }

            // Отправляем лог в канал
            if (_logChannel != null)
            {
                try
                {
                    var embed = new EmbedBuilder()
                        .WithTitle($"📋 Событие: {eventType}")
                        .WithDescription(description)
                        .AddField("Время", log.Timestamp.ToString("dd.MM.yyyy HH:mm:ss"))
                        .AddField("Пользователь", userName ?? "System")
                        .WithColor(GetColorForEventType(eventType))
                        .WithFooter($"Log ID: {_logs.Count}")
                        .Build();

                    await _logChannel.SendMessageAsync(embed: embed);
                }
                catch { }
            }
        }

        public List<ServerLog> GetLogs(int count = 10)
        {
            return _logs.TakeLast(count).Reverse().ToList();
        }

        public List<ServerLog> GetLogsByEventType(string eventType, int count = 10)
        {
            return _logs
                .Where(l => l.EventType == eventType)
                .TakeLast(count)
                .Reverse()
                .ToList();
        }

        public List<ServerLog> GetLogsByUser(ulong userId, int count = 10)
        {
            return _logs
                .Where(l => l.UserId == userId)
                .TakeLast(count)
                .Reverse()
                .ToList();
        }

        public List<ServerLog> GetAllLogs()
        {
            return _logs.ToList();
        }

        private Color GetColorForEventType(string eventType)
        {
            return eventType switch
            {
                "FamilyApplication" => Color.Green,
                "ApplicationApproved" => Color.Teal,
                "ApplicationRejected" => Color.Red,
                "TransportRequest" => Color.Magenta,
                "TransportApproved" => Color.Purple,
                "AutoSelection" => Color.Blue,
                "UserJoined" => Color.LighterGrey,
                "BotStartup" => Color.Gold,
                _ => Color.Default
            };
        }
    }
}
