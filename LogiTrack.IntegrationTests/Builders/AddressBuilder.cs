using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class AddressBuilder
    {

        private Address _address = new Address
        {
            Country = "Poland",
            City = "Gdańsk",
            Street = "Polanki 15",
            PostalCode = "14-784"
        };

        public AddressBuilder AsPickup()
        {
            _address = new Address
            {
                Id = 1,
                Country = "Poland",
                City = "Gdańsk",
                Street = "Polanki 15",
                PostalCode = "14-784"
            };
            return this;
        }

        public AddressBuilder AsDelivery()
        {
            _address = new Address
            {
                Id = 2,
                Country = "England",
                City = "London",
                Street = "Downing Street 1",
                PostalCode = "4444447"
            };
            return this;
        }

        public AddressBuilder WithId(int id)
        {
            _address.Id = id;
            return this;
        }

        public Address Build() => _address;
    }
}
