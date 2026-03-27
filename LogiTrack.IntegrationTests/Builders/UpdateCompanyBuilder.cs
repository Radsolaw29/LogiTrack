using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class UpdateCompanyBuilder
    {

        private UpdateCompanyDto _company = new UpdateCompanyDto
        {
            Name = "Test company update",
            Description = "Test description",
            TaxNumber = 111111111,
            ContactEmail = "testupdate@wp.pl"
        };

        public UpdateCompanyDto Build() => _company;
    }
}
