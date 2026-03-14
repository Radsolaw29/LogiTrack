using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class AddressesBuilder
    {

        private List<Address> _addresses = new List<Address>
        {
            new Address
            {
                Country = "Sweden",
                City = "Stockholm",
                Street = "Kungsgatan 15",
                PostalCode = "11-000",
                CreatedById = 1
            },

            new Address
            {
                Country = "Poland",
                City = "Sopot",
                Street = "Pańska 1",
                PostalCode = "11-885",
                CreatedById = 2
            },

            new Address
            {
                Country = "England",
                City = "London",
                Street = "Downing Street 123",
                PostalCode = "00-754",
                CreatedById = 3
            }
        };

        public List<Address> Build() => _addresses;

    }
}
