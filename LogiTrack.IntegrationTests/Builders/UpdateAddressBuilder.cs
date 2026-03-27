using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class UpdateAddressBuilder
    {

        private UpdateAddressDto _dto = new UpdateAddressDto
        {
            Country = "Poland",
            City = "Warszawa",
            Street = "Wojska Polskiego 12",
            PostalCode = "00-001"
        };

        public UpdateAddressDto Build() => _dto;
    }
}
