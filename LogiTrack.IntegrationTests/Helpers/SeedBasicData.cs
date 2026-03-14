using LogiTrack.IntegrationTests.Seed;
using Microsoft.AspNetCore.Mvc.Testing;
using NLog.Config;

namespace LogiTrack.IntegrationTests.Helpers
{
    public static class SeedBasicData
    {
        public static async Task SeedAsync(WebApplicationFactory<Program> factory)
        {
            await TestDbSeeder.ResetDatabase(factory);
            await TestDbSeeder.SeedBasicCompany(factory);
            await TestDbSeeder.SeedBasicOrderDependencies(factory);
            await TestDbSeeder.SeedBasicTransportOrders(factory);
        }
    }
}
