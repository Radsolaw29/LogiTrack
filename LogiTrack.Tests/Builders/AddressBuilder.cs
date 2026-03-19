using LogiTrack.Entities;

namespace LogiTrack.UnitTests.Builders
{
    public class AddressBuilder
    {
        private readonly Address _address;

        public AddressBuilder()
        {
            _address = new Address()
            {
                Id = 1,
                Country = "Poland",
                City = "Sopot",
                Street = "Aleja Zwycięstwa 115",
                PostalCode = "12345",
                CreatedById = 1
            };
        }

        public AddressBuilder WithId(int id)
        {
            _address.Id = id;
            return this;
        }

        public AddressBuilder WithCountry(string country)
        {
            _address.Country = country;
            return this;
        }

        public AddressBuilder WithCity(string city)
        {
            _address.City = city;
            return this;
        }

        public AddressBuilder WithStreet(string street)
        {
            _address.Street = street;
            return this;
        }

        public AddressBuilder WithPostalCode(string postalCode)
        {
            _address.PostalCode = postalCode;
            return this;
        }

        public AddressBuilder WithCreatedById(int CreatedById)
        {
            _address.CreatedById = CreatedById;
            return this;
        }

        public Address Build() => _address;
    }
}