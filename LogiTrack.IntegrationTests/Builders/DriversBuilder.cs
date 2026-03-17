using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class DriversBuilder
    {

        private List<Driver> _drivers = new List<Driver>
        {
            new Driver
            {
                Id = 1,
                FirstName = "Jan",
                LastName = "Kowalski",
                PersonalNumber = "11447754121",
                LicenseDriving = "C+E",
                ContactEmail = "Jan@wp.pl",
                CreatedById = 1,
                CompanyId = 1
            },

            new Driver
            {
                Id = 2,
                FirstName = "Karol",
                LastName = "Podolski",
                PersonalNumber = "55512541212",
                LicenseDriving = "C+E",
                ContactEmail = "Karol@wp.pl",
                CreatedById = 1,
                CompanyId = 1
            },

            new Driver
            {
                Id = 3,
                FirstName = "Adam",
                LastName = "Cymbal",
                PersonalNumber = "77777777777",
                LicenseDriving = "C+E",
                ContactEmail = "Adam@wp.pl",
                CreatedById = 1,
                CompanyId = 1
            }
        };

        public List<Driver> Build() => _drivers;

    }
}
