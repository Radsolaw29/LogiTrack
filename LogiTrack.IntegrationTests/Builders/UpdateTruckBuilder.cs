using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class UpdateTruckBuilder
    {

        private UpdateTruckDto _dto = new UpdateTruckDto()
        {
            RegistrationNumber = "GSP 55555",
            Brand = "Scania",
            Model = "FH10",
            Year = 2018,
            Mileage = 5000,
            CapacityTons = 11,
            Type = "Semi-trail truck"
            
        };

        public UpdateTruckDto Build() => _dto;
    }
}
