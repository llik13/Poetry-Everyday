using BusinessLogic.DTOs;
using FluentAssertions;
using Poetry.IntegrationTests.Extensions;
using Poetry.IntegrationTests.TestData;
using Presentation;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Poetry.IntegrationTests.Controllers
{
    public class UserPoemsControllerTests : IntegrationTestBase
    {
        public UserPoemsControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData("api/mypoems", "OK")]
        [InlineData("api/mypoems/drafts", "OK")]
        public async Task GetUserPoems_ReturnsSuccessStatusCode(string endpoint, string expectedStatus)
        {
            // Arrange
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());

            // Act
            var response = await Client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                if (endpoint == "api/mypoems")
                {
                    var poems = await response.Content.ReadFromJsonAsync<List<PoemDto>>();
                    poems.Should().NotBeNull();
                }
                else if (endpoint == "api/mypoems/drafts")
                {
                    var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PoemDto>>();
                    result.Should().NotBeNull();
                }
            }
        }

        [Theory]
        [InlineData("api/mypoems/{0}", "OK")]
        [InlineData("api/mypoems/nonexistent-id", "NotFound")]
        public async Task GetPoemById_ReturnsExpectedStatusCode(string urlTemplate, string expectedStatus)
        {
            // Arrange
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());

            var url = string.Format(urlTemplate, DatabaseSeeder.TestPoemId1);

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                var poem = await response.Content.ReadFromJsonAsync<PoemDetailsDto>();
                poem.Should().NotBeNull();
                poem!.Id.Should().Be(DatabaseSeeder.TestPoemId1);
            }
        }

        [Fact]
        public async Task CreatePoem_ReturnsCreatedResponse()
        {
            // Arrange
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());

            var newPoem = new CreatePoemDto
            {
                Title = "New Integration Test Poem",
                Content = "This is a poem created during integration testing.",
                Excerpt = "Integration test poem excerpt",
                IsPublished = false,
                Tags = new List<string> { "Test", "Integration" },
                Categories = new List<string> { "Test Category" }
            };

            // Act
            var response = await Client.PostAsJsonAsync("api/mypoems", newPoem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdPoem = await response.Content.ReadFromJsonAsync<PoemDto>();
            createdPoem.Should().NotBeNull();
            createdPoem!.Title.Should().Be(newPoem.Title);
            createdPoem.Content.Should().Be(newPoem.Content);
            createdPoem.AuthorId.Should().Be(DatabaseSeeder.TestUserId1);
            createdPoem.Tags.Should().Contain(newPoem.Tags);
        }

        [Theory]
        [InlineData("Unpublish", "{0}", "NoContent", true)]
        [InlineData("Unpublish", "nonexistent-id", "NotFound", true)]
        [InlineData("Unpublish", "{1}", "Forbidden", false)] // Try to unpublish another user's poem
        [InlineData("Publish", "{0}", "OK", true)]
        [InlineData("Publish", "nonexistent-id", "NotFound", true)]
        public async Task PoemStateOperations_ReturnExpectedResults(string operation, string idTemplate, string expectedStatus, bool useTestUser1)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());

            var id = idTemplate.Contains("{0}")
                ? string.Format(idTemplate, DatabaseSeeder.TestPoemId1)
                : idTemplate.Contains("{1}")
                    ? string.Format(idTemplate, DatabaseSeeder.TestPoemId2)
                    : idTemplate;

            var url = $"api/mypoems/{operation.ToLower()}/{id}";

            // Act
            HttpResponseMessage response;
            if (operation == "Unpublish" || operation == "Publish")
            {
                response = await Client.PutAsync(url, null);
            }
            else
            {
                response = await Client.GetAsync(url); // Fallback, shouldn't happen
            }

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                // For Publish, we should get a poem back
                if (operation == "Publish")
                {
                    var poem = await response.Content.ReadFromJsonAsync<PoemDto>();
                    poem.Should().NotBeNull();
                    poem!.IsPublished.Should().BeTrue("the poem should be published after the publish operation");
                }

                // For Unpublish, we can verify the state has changed
                if (operation == "Unpublish" && response.StatusCode == HttpStatusCode.NoContent)
                {
                    var poemResponse = await Client.GetAsync($"api/mypoems/{id}");
                    poemResponse.IsSuccessStatusCode.Should().BeTrue();

                    var poem = await poemResponse.Content.ReadFromJsonAsync<PoemDetailsDto>();
                    poem.Should().NotBeNull();
                    poem!.IsPublished.Should().BeFalse("the poem should be unpublished after the unpublish operation");
                }
            }
        }

        [Theory]
        [InlineData("{0}", true, "OK")]
        [InlineData("{0}", false, "Forbidden")] // Wrong user
        [InlineData("nonexistent-id", true, "NotFound")]
        public async Task UpdatePoem_ReturnsExpectedStatusCode(string idTemplate, bool useTestUser1, string expectedStatus)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());

            var id = idTemplate.Contains("{0}")
                ? string.Format(idTemplate, DatabaseSeeder.TestPoemId1)
                : idTemplate;

            var updateDto = new UpdatePoemDto
            {
                Id = Guid.Parse(id),
                Title = "Updated Title",
                Content = "Updated content from integration test",
                Excerpt = "Updated excerpt",
                IsPublished = true,
                Tags = new List<string> { "Updated", "Test" },
                Categories = new List<string> { "Updated Category" },
                ChangeNotes = "Integration test update"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"api/mypoems/{id}", updateDto);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                var poem = await response.Content.ReadFromJsonAsync<PoemDto>();
                poem.Should().NotBeNull();
                poem!.Title.Should().Be(updateDto.Title);
                poem.Content.Should().Be(updateDto.Content);
                poem.Excerpt.Should().Be(updateDto.Excerpt);
                poem.Tags.Should().Contain(updateDto.Tags);
            }
        }

        [Theory]
        [InlineData("{0}", true, "NoContent")]
        [InlineData("{0}", false, "Forbidden")] // Wrong user
        [InlineData("nonexistent-id", true, "NotFound")]
        public async Task DeletePoem_ReturnsExpectedStatusCode(string idTemplate, bool useTestUser1, string expectedStatus)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());

            var id = idTemplate.Contains("{0}")
                ? string.Format(idTemplate, DatabaseSeeder.TestPoemId1)
                : idTemplate;

            // Act
            var response = await Client.DeleteAsync($"api/mypoems/{id}");

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                // Verify the poem is deleted by trying to get it
                var poemResponse = await Client.GetAsync($"api/mypoems/{id}");
                poemResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}