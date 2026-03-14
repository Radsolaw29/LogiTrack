using LogiTrack.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LogiTrack.IntegrationTests.Extensions
{
    public static class TestClientFactoryExtensions
    {

        public static HttpClient CreateUnauthorizedClient(this WebApplicationFactory<Program> factory)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IPolicyEvaluator, UnauthenticatedPolicyEvaluator>();
                });
            }).CreateClient();
        }
    }
}
