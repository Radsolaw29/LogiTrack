using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class CreateTruckBuilder
    {

        private CreateTruckDto _dto = new CreateTruckDto
        {
            RegistrationNumber = "GD 15424",
            Brand = "Volvo",
            Model = "FH 16",
            Year = 2022,
            Mileage = 150000,
            CapacityTons = 22,
            Type = "Semi-trail truck",
            CompanyId = 1
        };

        public CreateTruckDto Build() => _dto;
    }
}
