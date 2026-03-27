using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class CreateAddressBuilder
    {

        private CreateAddressDto _dto = new CreateAddressDto
        {
            Country = "Poland",
            City = "Warsaw",
            Street = "Test 1",
            PostalCode = "00-001"
        };

        public CreateAddressDto Build() => _dto;

    }
}
