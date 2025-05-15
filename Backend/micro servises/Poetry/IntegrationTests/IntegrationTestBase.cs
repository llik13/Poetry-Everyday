using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Presentation; // Add reference to Presentation namespace

namespace Poetry.IntegrationTests
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        protected readonly CustomWebApplicationFactory<Program> Factory;
        protected HttpClient Client { get; }

        protected IntegrationTestBase(CustomWebApplicationFactory<Program> factory)
        {
            Factory = factory;
            Client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        protected void AuthenticateClient(string userId, string userName = "Test User", string role = "User")
        {
            Client.DefaultRequestHeaders.Add("User-Id", userId);
            Client.DefaultRequestHeaders.Add("User-Name", userName);
            Client.DefaultRequestHeaders.Add("User-Role", role);
        }

        protected StringContent CreateJsonContent<T>(T content)
        {
            var json = JsonSerializer.Serialize(content);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected async Task<T?> ReadFromJsonAsync<T>(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}