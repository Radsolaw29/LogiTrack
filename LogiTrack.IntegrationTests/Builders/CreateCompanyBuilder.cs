using LogiTrack.Entities;
using LogiTrack.Models;

namespace LogiTrack.IntegrationTests.Builders
{
    public class CreateCompanyBuilder
    {

        private CreateCompanyDto _company = new CreateCompanyDto
        {
            Name = "CompanyTest123",
            Description = "Description Test123",
            TaxNumber = 1235469877,
            PhoneNumber = 111222333,
            ContactEmail = "testcompany@wp.pl",
            Country = "Poland",
            City = "Sopot",
            Street = "Łokietka 17c/1",
            PostalCode = "12345"
        };

        public CreateCompanyDto Build() => _company;
    }
}
