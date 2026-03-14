using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class DriverBuilder
    {

        private Driver _driver = new Driver
        {
            Id = 1,
            FirstName = "Jan",
            LastName = "Kowalski",
            PersonalNumber = "11447754121",
            LicenseDriving = "C+E",
            CreatedById = 1,
        };

        public Driver Build() => _driver;

        public DriverBuilder WithId(int id)
        {
            _driver.Id = id;
            return this;
        }
    }
}
