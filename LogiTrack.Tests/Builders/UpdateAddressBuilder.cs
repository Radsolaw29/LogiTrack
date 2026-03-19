using LogiTrack.Models;

namespace LogiTrack.UnitTests.Builders
{
    public class UpdateAddressBuilder
    {
        private string _country = "Portugal";
        private string _city = "Lizbona";
        private string _street = "Polna 147";
        private string _postalCode = "12345";

        public UpdateAddressBuilder WithCountry(string country)
        {
            _country = country; 
            return this;
        }

        public UpdateAddressBuilder WithCity(string city)
        {
            _city = city;
            return this;
        }

        public UpdateAddressBuilder WithStreet(string street)
        {
            _street = street;
            return this;
        }

        public UpdateAddressBuilder WithPostalCode(string postalCode)
        {
            _postalCode = postalCode;
            return this;
        }

        public UpdateAddressDto Build()
        {
            return new UpdateAddressDto
            {
                Country = _country,
                City = _city,
                Street = _street,
                PostalCode = _postalCode
            };
        }
    }
}