using Newtonsoft.Json;

namespace LogiTrack.IntegrationTests.Extensions
{
    public static class HttpJsonExtensions
    {

        public static async Task<T> DeserializeAsync<T>(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content) ??
                throw new Exception("Failed to deserialize response");
        }
    }
}
