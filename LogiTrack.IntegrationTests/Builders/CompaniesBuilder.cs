using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class CompaniesBuilder
    {

        private List<Company> _companies = new List<Company>
        {
                new Company
                {
                    Id = 1,
                    Name = "Test name123",
                    Description = "Test desc 1",
                    ContactEmail = "c@test.com",
                    CreatedById = 1,

                    Address = new Address { Country = "Poland", City = "Sopot", Street = "Pańska 1", PostalCode = "11-885" }
                },

                new Company
                {
                    Id = 2,
                    Name = "Test name456",
                    Description = "Test desc 2",
                    ContactEmail = "b@test.com",
                    CreatedById = 2,

                    Address = new Address { Country = "Poland", City = "Gdynia", Street = "Wojska Polskiego 2", PostalCode = "11-800" }
                },

                new Company
                {
                    Id = 3,
                    Name = "Test name789",
                    Description = "Test desc 3",
                    ContactEmail = "a@test.com",
                    CreatedById = 3,

                    Address = new Address { Country = "Poland", City = "Gdańsk", Street = "Długa 12", PostalCode = "80-885" }
                }
        };

        public List<Company> Build() => _companies;

    }
}
