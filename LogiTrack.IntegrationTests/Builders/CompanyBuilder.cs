using LogiTrack.Entities;

namespace LogiTrack.IntegrationTests.Builders
{
    public class CompanyBuilder
    {

        private Company _company = new Company
        {
            Id = 1,
            Name = "First company",
            Description = "Test company",
            CreatedById = 1
        };

        public Company Build() => _company;

        public CompanyBuilder WithId(int id)
        {
            _company.Id = id;
            return this;
        }

    }
}
