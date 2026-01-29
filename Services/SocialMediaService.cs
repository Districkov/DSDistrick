using DiscordBot.Models;

namespace DiscordBot.Services
{
    public class SocialMediaService
    {
        private List<SocialMediaAccount> _accounts = new();
        private Dictionary<string, DateTime> _lastChecked = new();

        public SocialMediaService()
        {
            _lastChecked["twitch"] = DateTime.MinValue;
            _lastChecked["youtube"] = DateTime.MinValue;
            _lastChecked["tiktok"] = DateTime.MinValue;
        }

        public void AddAccount(string platform, string channelName)
        {
            var account = new SocialMediaAccount
            {
                Platform = platform,
                ChannelName = channelName,
                IsActive = true,
                LastChecked = DateTime.Now
            };

            var existing = _accounts.FirstOrDefault(a => a.Platform == platform);
            if (existing != null)
            {
                _accounts.Remove(existing);
            }

            _accounts.Add(account);
        }

        public SocialMediaAccount GetAccount(string platform)
        {
            return _accounts.FirstOrDefault(a => a.Platform.ToLower() == platform.ToLower());
        }

        public List<SocialMediaAccount> GetAllAccounts()
        {
            return _accounts.ToList();
        }

        public void UpdateLastChecked(string platform)
        {
            var account = GetAccount(platform);
            if (account != null)
            {
                account.LastChecked = DateTime.Now;
            }
        }

        public DateTime GetLastChecked(string platform)
        {
            if (_lastChecked.TryGetValue(platform.ToLower(), out var lastCheck))
            {
                return lastCheck;
            }
            return DateTime.MinValue;
        }

        public void SetLastChecked(string platform, DateTime dateTime)
        {
            _lastChecked[platform.ToLower()] = dateTime;
        }
    }
}
