using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class CreateDriverBuilder
    {

        private CreateDriverDto _dto = new CreateDriverDto
        {
            FirstName = "Julian",
            LastName = "Julek",
            PersonalNumber = "12312457845",
            DateOfBirth = new DateTime (1985-05-19),
            LicenseDriving = "C+E",
            PhoneNumber = 111222555,
            ContactEmail = "Julian@wp.pl",
            CompanyId = 1
        };

        public CreateDriverDto Build() => _dto;
    }
}
