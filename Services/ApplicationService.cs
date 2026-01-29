using DiscordBot.Models;

namespace DiscordBot.Services
{
    public class ApplicationService
    {
        private List<FamilyApplication> _applications = new();

        public void AddApplication(FamilyApplication app)
        {
            _applications.Add(app);
        }

        public void UpdateApplicationStatus(ulong userId, string status)
        {
            var app = _applications.FirstOrDefault(a => a.UserId == userId);
            if (app != null)
            {
                app.Status = status;
            }
        }

        public FamilyApplication GetApplication(ulong userId)
        {
            return _applications.FirstOrDefault(a => a.UserId == userId);
        }

        public List<FamilyApplication> GetAllApplications()
        {
            return _applications.ToList();
        }

        public List<FamilyApplication> GetPendingApplications()
        {
            return _applications.Where(a => a.Status == "Pending").ToList();
        }
    }
}
