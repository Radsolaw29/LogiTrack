using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogiTrack.IntegrationTests.Helpers
{
    public static class TestClientFactoryExtensions
    {

        public static HttpClient CreateClientWithRole(this WebApplicationFactory<Program> factory, string role)
        {
            var client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                });
            }).CreateClient();

            // Dodajemy rolę do nagłówka - Evaluator ją wyłapie przed autoryzacją
            client.DefaultRequestHeaders.Add("Test-Role", role);

            return client;
        }
    }
}
