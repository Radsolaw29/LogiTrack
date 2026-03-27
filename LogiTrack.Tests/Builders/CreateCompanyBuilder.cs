using LogiTrack.Models;

namespace LogiTrack.UnitTests.Builders
{
    public class CreateCompanyBuilder
    {
        private string _name = "Eagle Trans";
        private string _description = "A transport company serving all of Europe.";
        private int _taxNumber = 123456789;
        private int _phoneNumber = 123456789;
        private string _contactEmail = "transport@wp.pl";
        private string _country = "Poland";
        private string _city = "Warszawa";
        private string _street = "Grunwaldzka 11";
        private string _postalCode = "00-762";

        public CreateCompanyBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CreateCompanyBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public CreateCompanyBuilder WithTaxNumber(int taxNumber)
        {
            _taxNumber = taxNumber;
            return this;
        }

        public CreateCompanyBuilder WithPhoneNumber(int phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public CreateCompanyBuilder WithContactEmail(string contactEmail)
        {
            _contactEmail = contactEmail;
            return this;
        }

        public CreateCompanyBuilder WithCountry(string country)
        {
            _country = country;
            return this;
        }

        public CreateCompanyBuilder WithCity(string city)
        {
            _city = city;
            return this;
        }

        public CreateCompanyBuilder WithStreet(string street)
        {
            _street = street;
            return this;
        }

        public CreateCompanyBuilder WithPostalCode(string postalCode)
        {
            _postalCode = postalCode;
            return this;
        }

        public CreateCompanyDto Build()
        {
            return new CreateCompanyDto
            {
                Name = _name,
                Description = _description,
                TaxNumber = _taxNumber,
                PhoneNumber = _phoneNumber,
                ContactEmail = _contactEmail,
                Country = _country,
                City = _city,
                Street = _street,
                PostalCode = _postalCode
            };
        }
    }
}