using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class TruckBuilder
    {

        private Truck _truck = new Truck
        {
            Id = 1,
            Brand = "Volvo",
            Model = "FH16",
            Year = 2020,
            RegistrationNumber = "GD12345",
            CompanyId = 1
        };

        public Truck Build() => _truck;

        public TruckBuilder WithId(int id)
        {
            _truck.Id = id;
            return this;
        }
    }
}


