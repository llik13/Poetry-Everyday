using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Poetry.IntegrationTests.Extensions
{
    public static class HttpClientExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            return client.PostAsJsonAsync(requestUri, value, JsonOptions);
        }

        public static Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            return client.PutAsJsonAsync(requestUri, value, JsonOptions);
        }

        public static async Task<T?> ReadFromJsonAsync<T>(this HttpContent content)
        {
            return await content.ReadFromJsonAsync<T>(JsonOptions);
        }
    }
}