using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class UpdateDriverBuilder
    {

        private UpdateDriverDto _dto = new UpdateDriverDto
        {
            FirstName = "Mikołaj",
            LastName = "Kowalski",
            PersonalNumber = "18854569874",
            DateOfBirth = new DateTime(1990-12-01),
            LicenseDriving = "C",
            PhoneNumber = 777444111,
            ContactEmail = "JasFasola@wp.pl"
        };

        public UpdateDriverDto Build() => _dto;
    }
}
