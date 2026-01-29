namespace DiscordBot.Models
{
    public class FamilyApplication
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }
        public string NickName { get; set; }
        public string FullName { get; set; }
        public string OnlineStatus { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
    }

    public class TransportRequest
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }
        public string VehicleType { get; set; }
        public string Destination { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime CreatedAt { get; set; }
    }

    public class CarOption
    {
        public string Name { get; set; }
        public bool IsOccupied { get; set; }
        public ulong? OccupiedBy { get; set; }
        public string OccupiedByName { get; set; }
        public DateTime? OccupiedAt { get; set; }
    }

    public class AutoSelection
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }
        public string SelectedCar { get; set; }
        public DateTime SelectedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ServerLog
    {
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Description { get; set; }
        public ulong? UserId { get; set; }
        public string UserName { get; set; }
    }

    public class SocialMediaAccount
    {
        public string Platform { get; set; } // Twitch, YouTube, TikTok
        public string ChannelName { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastChecked { get; set; }
    }

    public class BotConfig
    {
        public string BotToken { get; set; }
        public ulong ApplicationId { get; set; }
        public ulong FamilyApplicationsChannelId { get; set; }
        public ulong TransportRequestChannelId { get; set; }
        public ulong ApplicationApprovalsChannelId { get; set; }
        public ulong ServerLogsChannelId { get; set; }
        public ulong SocialMediaChannelId { get; set; }
        public string TwitchChannelId { get; set; }
        public string YouTubeChannelId { get; set; }
        public string TikTokUsername { get; set; }
    }
}
