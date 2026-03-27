using LogiTrack.Models;

namespace LogiTrack.UnitTests.Builders
{
    public class CreateDriverBuilder
    {
        private string _firstName = "Jan";
        private string _lastName = "Nowak";
        private string _personalNumber = "12345678901";
        private DateTime _dateOfBirth = new DateTime(1990, 1, 1);
        private string _licenseDriving = "C+E";
        private int _phoneNumber = 123456789;
        private string _contactEmail = "mikolajki@wp.pl";
        private int _companyId = 1;

        public CreateDriverBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public CreateDriverBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public CreateDriverBuilder WithPersonalNumber(string personalNumber)
        {
            _personalNumber = personalNumber;
            return this;
        }

        public CreateDriverBuilder WithDateOfBirth(DateTime dateOfBirth)
        {
            _dateOfBirth = dateOfBirth;
            return this;
        }

        public CreateDriverBuilder WithLicenseDriving(string licenseDriving)
        {
            _licenseDriving = licenseDriving;
            return this;
        }

        public CreateDriverBuilder WithPhoneNumber(int phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public CreateDriverBuilder WithContactEmail(string contactEmail)
        {
            _contactEmail = contactEmail;
            return this;
        }

        public CreateDriverBuilder WithCompanyId(int companyId)
        {
            _companyId = companyId;
            return this;
        }

        public CreateDriverDto Build()
        {
            return new CreateDriverDto
            {
                FirstName = _firstName,
                LastName = _lastName,
                PersonalNumber = _personalNumber,
                DateOfBirth = _dateOfBirth,
                LicenseDriving = _licenseDriving,
                PhoneNumber = _phoneNumber,
                ContactEmail = _contactEmail,
                CompanyId = _companyId
            };
        }
    }
}