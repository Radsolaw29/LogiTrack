using LogiTrack.Models;

namespace LogiTrack.UnitTests.Builders
{
    public class UpdateDriverBuilder
    {
        private string _firstName = "Krzyś";
        private string _lastName = "Matijas";
        private string _personalNumber = "11122231111";
        private DateTime _dateOfBirth = new DateTime(1980, 5, 21);
        private string _licenseDriving = "C";
        private int _phoneNumber = 987654111;
        private string _contactEmail = "krzysztof@wp.pl";

        public UpdateDriverBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public UpdateDriverBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public UpdateDriverBuilder WithPersonalNumber(string personalNumber)
        {
            _personalNumber = personalNumber;
            return this;
        }

        public UpdateDriverBuilder WithDateOfBirth(DateTime dateOfBirth)
        {
            _dateOfBirth = dateOfBirth;
            return this;
        }

        public UpdateDriverBuilder WithLicenseDriving(string licenseDriving)
        {
            _licenseDriving= licenseDriving;
            return this;
        }

        public UpdateDriverBuilder WithPhoneNumber(int phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public UpdateDriverBuilder WithContactEmail(string contactEmail)
        {
            _contactEmail = contactEmail;
            return this;
        }

        public UpdateDriverDto Build()
        {
            return new UpdateDriverDto
            {
                FirstName= _firstName,
                LastName= _lastName,
                PersonalNumber= _personalNumber,
                DateOfBirth= _dateOfBirth,
                LicenseDriving= _licenseDriving,
                PhoneNumber= _phoneNumber,
                ContactEmail= _contactEmail
            };
        }
    }
}