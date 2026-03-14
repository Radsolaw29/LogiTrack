using LogiTrack.Entities;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogiTrack.IntegrationTests.Extensions
{
    public static class TestDbExtensions
    {

        public static async Task SeedAsync<T>(this WebApplicationFactory<Program> factory, T entity) where T : class
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LogiTrackDbContext>();

            db.Set<T>().Add(entity);
            await db.SaveChangesAsync();
        }

    }
}
