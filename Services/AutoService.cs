using DiscordBot.Models;

namespace DiscordBot.Services
{
    public class AutoService
    {
        private List<AutoSelection> _selections = new();
        private List<CarOption> _cars = new();

        public AutoService()
        {
            InitializeCars();
        }

        private void InitializeCars()
        {
            _cars = new()
            {
                new CarOption { Name = "🚗 BMW M5", IsOccupied = false },
                new CarOption { Name = "🏎️ Ferrari 488", IsOccupied = false },
                new CarOption { Name = "🚙 Mercedes G-Class", IsOccupied = false },
                new CarOption { Name = "🏁 Lamborghini Huracán", IsOccupied = false },
                new CarOption { Name = "⚡ Porsche 911", IsOccupied = false },
                new CarOption { Name = "🚕 Tesla Model S", IsOccupied = false },
                new CarOption { Name = "🚐 Range Rover", IsOccupied = false },
                new CarOption { Name = "💨 Bugatti Chiron", IsOccupied = false }
            };
        }

        public List<CarOption> GetAllCars()
        {
            return _cars.ToList();
        }

        public List<CarOption> GetAvailableCars()
        {
            return _cars.Where(c => !c.IsOccupied).ToList();
        }

        public List<CarOption> GetOccupiedCars()
        {
            return _cars.Where(c => c.IsOccupied).ToList();
        }

        public void SelectCar(ulong userId, string userName, string carName)
        {
            var car = _cars.FirstOrDefault(c => c.Name == carName);
            if (car != null)
            {
                car.IsOccupied = true;
                car.OccupiedBy = userId;
                car.OccupiedByName = userName;
                car.OccupiedAt = DateTime.Now;
            }

            var selection = new AutoSelection
            {
                UserId = userId,
                Username = userName,
                SelectedCar = carName,
                SelectedAt = DateTime.Now,
                IsActive = true
            };

            var existing = _selections.FirstOrDefault(s => s.UserId == userId);
            if (existing != null)
            {
                _selections.Remove(existing);
            }

            _selections.Add(selection);
        }

        public void FreeCar(ulong userId)
        {
            var car = _cars.FirstOrDefault(c => c.OccupiedBy == userId && c.IsOccupied);
            if (car != null)
            {
                car.IsOccupied = false;
                car.OccupiedBy = null;
                car.OccupiedByName = null;
                car.OccupiedAt = null;
            }

            var selection = _selections.FirstOrDefault(s => s.UserId == userId && s.IsActive);
            if (selection != null)
            {
                selection.IsActive = false;
            }
        }

        public AutoSelection GetUserSelection(ulong userId)
        {
            return _selections.FirstOrDefault(s => s.UserId == userId && s.IsActive);
        }

        public List<AutoSelection> GetAllSelections()
        {
            return _selections.Where(s => s.IsActive).ToList();
        }
    }
}
