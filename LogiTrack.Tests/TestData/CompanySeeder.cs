using LogiTrack.Entities;
using LogiTrack.UnitTests.Builders;

namespace LogiTrack.UnitTests.TestData
{
    public static class CompanySeeder
    {
        public static void SeedCompanies(LogiTrackDbContext dbContext)
        {
            var companies = new List<Company>
            {
                new CompanyBuilder()
                    .WithId(1)
                    .WithName("Eagle Trans")
                    .WithDescription("A transport company serving all of Europe.")
                    .WithTaxNumber(123456789)
                    .WithPhoneNumber(123456789)
                    .WithEmail("transport@wp.pl")
                    .WithAddress(new Address
                    {
                        Id = 1,
                        Country = "Poland",
                        City = "Warszawa",
                        Street = "Marszałkowska 1",
                        PostalCode = "00-101"
                    })
                    .Build(),

                new CompanyBuilder()
                    .WithId(2)
                    .WithName("Nordic Logistics")
                    .WithDescription("Scandinavian transport services.")
                    .WithTaxNumber(987654321)
                    .WithPhoneNumber(987654321)
                    .WithEmail("contact@nordic.com")
                    .WithAddress(new Address
                    {
                        Id = 2,
                        Country = "Sweden",
                        City = "Stockholm",
                        Street = "Sveavägen 10",
                        PostalCode = "111 57"
                    })
                    .Build(),

                new CompanyBuilder()
                    .WithId(3)
                    .WithName("Norway Logistics")
                    .WithDescription("Scandinavian transport services.")
                    .WithTaxNumber(987654333)
                    .WithPhoneNumber(987654333)
                    .WithEmail("norway@nordic.com")
                    .WithAddress(new Address
                    {
                        Id = 3,
                        Country = "Norway",
                        City = "Oslo",
                        Street = "Karl Johans gate 15",
                        PostalCode = "100 01"
                    })
                    .Build()
            };

            dbContext.Companies.AddRange(companies);
            dbContext.SaveChanges();
        }
    }
}