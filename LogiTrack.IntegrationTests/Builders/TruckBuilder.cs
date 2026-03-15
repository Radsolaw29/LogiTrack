using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class TruckBuilder
    {

        private List<Truck> _trucks = new List<Truck>
        {
            new Truck
            {
                Id = 1,
                Brand = "Volvo",
                Model = "FH16",
                Year = 2020,
                RegistrationNumber = "GD12345",
                CompanyId = 1,
                CreatedById = 1
            },

            new Truck
            {
                Id = 2,
                Brand = "Saab",
                Model = "FH18",
                Year = 2025,
                RegistrationNumber = "GD11111",
                CompanyId = 1
            },

            new Truck
            {
                Id = 3,
                Brand = "Mercedes",
                Model = "FH20",
                Year = 2026,
                RegistrationNumber = "GD22222",
                CompanyId = 1
            }
            
        };

        public List<Truck> Build() => _trucks;
    }
}
