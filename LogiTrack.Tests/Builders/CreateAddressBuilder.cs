using LogiTrack.Models;

namespace LogiTrack.UnitTests.Builders
{
    public class CreateAddressBuilder
    {
        private string _country = "Portugal";
        private string _city = "Lizbona";
        private string _street = "Polna 147";
        private string _postalCode = "12345";

        public CreateAddressBuilder WithCountry(string country)
        {
            _country = country;
            return this;
        }

        public CreateAddressBuilder WithCity(string city)
        {
            _city = city; 
            return this;
        }

        public CreateAddressBuilder WithStreet(string street)
        {
            _street = street;
            return this;
        }

        public CreateAddressBuilder WithPostalCode(string postalCode)
        {
            _postalCode = postalCode;
            return this;
        }

        public CreateAddressDto Build()
        {
            return new CreateAddressDto
            {
                Country = _country,
                City = _city,
                Street = _street,
                PostalCode = _postalCode
            };
        }
    }
}