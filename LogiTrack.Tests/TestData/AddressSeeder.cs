using LogiTrack.Entities;
using LogiTrack.UnitTests.Builders;

namespace LogiTrack.UnitTests.TestData
{
    public static class AddressSeeder
    {
        public static void SeedAddress(LogiTrackDbContext dbContext)
        {
            var addresses = new List<Address>
            {
                new AddressBuilder()
                .WithId(1)
                .WithCountry("Poland")
                .WithCity("Sopot")
                .WithStreet("Aleja Zwycięstwa 115")
                .WithPostalCode("12345")
                .WithCreatedById(1)
                .Build(),

                new AddressBuilder()
                .WithId(2)
                .WithCountry("England")
                .WithCity("London")
                .WithStreet("Oxford Street 15")
                .WithPostalCode("55777")
                .WithCreatedById(2)
                .Build(),

                new AddressBuilder()
                .WithId(3)
                .WithCountry("Sweeden")
                .WithCity("Stockholm")
                .WithStreet("ADrottninggatan 22")
                .WithPostalCode("11111")
                .WithCreatedById(3)
                .Build(),

                new AddressBuilder()
                .WithId(4)
                .WithCountry("Sweeden")
                .WithCity("Stockholm")
                .WithStreet("Karls 74")
                .WithPostalCode("11114")
                .WithCreatedById(4)
                .Build(),
            };

            dbContext.Addresses.AddRange(addresses);
            dbContext.SaveChanges();
        }
    }
}