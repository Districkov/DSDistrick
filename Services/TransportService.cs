using DiscordBot.Models;

namespace DiscordBot.Services
{
    public class TransportService
    {
        private List<TransportRequest> _requests = new();
        
        private readonly string[] _vehicleTypes = new[]
        {
            "🚗 Седан",
            "🚙 Внедорожник",
            "🚐 Микроавтобус",
            "🏎️ Спортивный автомобиль",
            "🚕 Такси",
            "🛻 Грузовик",
            "🏍️ Мотоцикл"
        };

        public string[] GetVehicleTypes()
        {
            return _vehicleTypes;
        }

        public void AddRequest(TransportRequest request)
        {
            _requests.Add(request);
        }

        public void UpdateRequestStatus(ulong userId, string status)
        {
            var request = _requests.FirstOrDefault(r => r.UserId == userId);
            if (request != null)
            {
                request.Status = status;
            }
        }

        public TransportRequest GetRequest(ulong userId)
        {
            return _requests.FirstOrDefault(r => r.UserId == userId);
        }

        public List<TransportRequest> GetAllRequests()
        {
            return _requests.ToList();
        }

        public List<TransportRequest> GetPendingRequests()
        {
            return _requests.Where(r => r.Status == "Pending").ToList();
        }

        public List<TransportRequest> GetApprovedRequests()
        {
            return _requests.Where(r => r.Status == "Approved").ToList();
        }
    }
}
