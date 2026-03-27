using LogiTrack.Entities;

namespace LogiTrack.UnitTests.Builders
{
    public class CompanyBuilder
    {
        private readonly Company _company;

        public CompanyBuilder()
        {
            _company = new Company
            {
                Id = 1,
                Name = "Eagle Trans",
                Description = "A transport company serving all of Europe.",
                TaxNumber = 123456789,
                PhoneNumber = 123456789,
                ContactEmail = "transport@wp.pl",
                Address = new Address
                {
                    Id = 1,
                    Country = "Poland",
                    City = "Warszawa",
                    Street = "Marszałkowska 1",
                    PostalCode = "00-101"
                },
                Trucks = new List<Truck>(),
                Drivers = new List<Driver>(),
                TransportOrders = new List<TransportOrder>()
            };
        }

        public CompanyBuilder WithId(int id)
        {
            _company.Id = id;
            return this;
        }

        public CompanyBuilder WithName(string name)
        {
            _company.Name = name;
            return this;
        }

        public CompanyBuilder WithDescription(string description)
        {
            _company.Description = description;
            return this;
        }

        public CompanyBuilder WithTaxNumber(int taxNumber)
        {
            _company.TaxNumber = taxNumber;
            return this;
        }

        public CompanyBuilder WithPhoneNumber(int phoneNumber)
        {
            _company.PhoneNumber = phoneNumber;
            return this;
        }

        public CompanyBuilder WithEmail(string email)
        {
            _company.ContactEmail = email;
            return this;
        }

        public CompanyBuilder WithAddress(Address address)
        {
            _company.Address = address;
            return this;
        }

        public Company Build() => _company;
    }
}