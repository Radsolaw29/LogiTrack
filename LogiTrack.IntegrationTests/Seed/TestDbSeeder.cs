using LogiTrack.Entities;
using LogiTrack.IntegrationTests.Builders;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogiTrack.IntegrationTests.Seed
{
    public static class TestDbSeeder
    {
        private static LogiTrackDbContext GetDbContext(WebApplicationFactory<Program> factory)
        {
            var scope = factory.Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();
        }

        public static async Task ResetDatabase(WebApplicationFactory<Program> factory)
        {
            using var db = GetDbContext(factory);

            db.Companies.RemoveRange(db.Companies);
            db.Addresses.RemoveRange(db.Addresses);
            db.Trucks.RemoveRange(db.Trucks);
            db.Drivers.RemoveRange(db.Drivers);
            db.Orders.RemoveRange(db.Orders);

            await db.SaveChangesAsync();
        }

        public static async Task SeedBasicCompany(WebApplicationFactory<Program> factory)
        {
            using var db = GetDbContext(factory);

            db.Companies.Add(new CompanyBuilder().Build());
            await db.SaveChangesAsync();
        }

        public static async Task SeedBasicCompanies(WebApplicationFactory<Program> factory) 
        {
            using var db = GetDbContext(factory);

            db.Companies.AddRange(new CompaniesBuilder().Build());
            await db.SaveChangesAsync();
        }

        public static async Task SeedBasicTransportOrder(WebApplicationFactory<Program> factory)
        {
            using var db = GetDbContext(factory);

            db.Orders.Add(new TransportOrderBuilder().Build());
            await db.SaveChangesAsync();
        }

        public static async Task SeedBasicTransportOrders(WebApplicationFactory<Program> factory)
        {
            using var db = GetDbContext(factory);

            db.Orders.AddRange(new TransportOrdersBuilder().Build());
            await db.SaveChangesAsync();
        }

        public static async Task SeedBasicAddresses(WebApplicationFactory<Program> factory)
        {
            using var db = GetDbContext(factory);

            db.Addresses.AddRange(new AddressesBuilder().Build());
            await db.SaveChangesAsync();
        }

        public static async Task SeedBasicOrderDependencies(WebApplicationFactory<Program> factory)
        {
            using var db = GetDbContext(factory);

            db.Addresses.Add(new AddressBuilder().AsPickup().Build());
            db.Addresses.Add(new AddressBuilder().AsDelivery().Build());
            db.Drivers.Add(new DriverBuilder().Build());
            db.Trucks.AddRange(new TruckBuilder().Build());

            await db.SaveChangesAsync();
        }
    }
}
