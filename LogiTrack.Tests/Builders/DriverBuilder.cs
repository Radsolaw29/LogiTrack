using LogiTrack.Entities;

namespace LogiTrack.UnitTests.Builders
{
    public class DriverBuilder
    {
        private readonly Driver _driver;

        public DriverBuilder()
        {
            _driver = new Driver
            {
                Id = 9,
                FirstName = "Krzysztof",
                LastName = "Krawczyk",
                PersonalNumber = "11122233344",
                DateOfBirth = new DateTime(1980, 5, 15),
                LicenseDriving = "C+E",
                PhoneNumber = 987654321,
                ContactEmail = "krzyszdzis@wp.pl",
                CompanyId = 1, 
                Company = new Company
                {
                    Id = 1
                }
            };
        }

        public DriverBuilder WithId(int id)
        {
            _driver.Id = id;
            return this;
        }

        public DriverBuilder WithFirstName(string firstName)
        {
            _driver.FirstName = firstName;
            return this;
        }

        public DriverBuilder WithLastName(string lastName) 
        { 
            _driver.LastName = lastName;
            return this;
        }

        public DriverBuilder WithPersonalNumber(string personalNumber)
        {
            _driver.PersonalNumber = personalNumber;
            return this;
        }

        public DriverBuilder WithDateOfBirth(DateTime dateOfBirth)
        {
            _driver.DateOfBirth = dateOfBirth;
            return this;
        }

        public DriverBuilder WithLicenseDriving(string licenseDriving)
        {
            _driver.LicenseDriving = licenseDriving;
            return this;
        }

        public DriverBuilder WithPhoneNumber(int phoneNumber)
        {
            _driver.PhoneNumber = phoneNumber;
            return this;
        }

        public DriverBuilder WithContactEmail(string contactEmail)
        {
            _driver.ContactEmail = contactEmail;
            return this;
        }

        public DriverBuilder WithCompanyId(int id)
        {
            _driver.CompanyId = id;
            return this;
        }

        public DriverBuilder WithCompany(Company company)
        {
            _driver.Company = company;
            _driver.CompanyId = company.Id;
            return this;
        }

        public Driver Build() => _driver;
    }
}