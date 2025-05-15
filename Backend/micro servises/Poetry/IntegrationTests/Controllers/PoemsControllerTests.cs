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
    public class PoemsControllerTests : IntegrationTestBase
    {
        public PoemsControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData("api/poems")]
        [InlineData("api/poems?pageNumber=1&pageSize=10")]
        [InlineData("api/poems?searchTerm=test")]
        public async Task GetPoems_ReturnsSuccessStatusCode(string endpoint)
        {
            // Act
            var response = await Client.GetAsync(endpoint);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            var result = await response.Content.ReadFromJsonAsync<PaginatedResult<PoemDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("api/poems/author/{0}", "OK")]
        [InlineData("api/poems/poemDetailed/{1}", "OK")]
        [InlineData("api/poems/content/{1}", "OK")]
        [InlineData("api/poems/nonexistent-id", "NotFound")]
        public async Task GetPoemEndpoints_ReturnsExpectedStatusCode(string urlTemplate, string expectedStatus)
        {
            // Arrange
            var url = string.Format(urlTemplate,
                DatabaseSeeder.TestUserId1,
                DatabaseSeeder.TestPoemId1);

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                if (urlTemplate.Contains("author"))
                {
                    var result = await response.Content.ReadFromJsonAsync<List<PoemDto>>();
                    result.Should().NotBeNull();
                }
                else if (urlTemplate.Contains("poemDetailed"))
                {
                    var result = await response.Content.ReadFromJsonAsync<PoemDetailsDto>();
                    result.Should().NotBeNull();
                    result!.Id.Should().Be(DatabaseSeeder.TestPoemId1);
                }
                else if (urlTemplate.Contains("content"))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    content.Should().NotBeNullOrEmpty();
                }
            }
        }

        [Theory]
        [InlineData("api/poems/comments/{0}", "OK")]
        [InlineData("api/poems/comments/nonexistent-id", "OK")] // Returns empty list, not 404
        public async Task GetPoemComments_ReturnsExpectedResults(string urlTemplate, string expectedStatus)
        {
            // Arrange
            var url = string.Format(urlTemplate, DatabaseSeeder.TestPoemId1);

            // Act
            var response = await Client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                var comments = await response.Content.ReadFromJsonAsync<List<CommentDto>>();
                comments.Should().NotBeNull();

                if (urlTemplate.Contains(DatabaseSeeder.TestPoemId1.ToString()))
                {
                    comments.Should().NotBeEmpty();
                }
            }
        }

        [Theory]
        [InlineData("api/poems/comments/{0}", "This is a new comment", "Created")]
        [InlineData("api/poems/comments/nonexistent-id", "This is a new comment", "NotFound")]
        public async Task AddComment_ReturnsExpectedStatusCode(string urlTemplate, string commentText, string expectedStatus)
        {
            // Arrange
            var url = string.Format(urlTemplate, DatabaseSeeder.TestPoemId1);
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());

            // Act
            var content = CreateJsonContent(commentText);
            var response = await Client.PostAsync(url, content);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode)
            {
                var comment = await response.Content.ReadFromJsonAsync<CommentDto>();
                comment.Should().NotBeNull();
                comment!.Text.Should().Be(commentText);
                comment.PoemId.Should().Be(DatabaseSeeder.TestPoemId1);
                comment.UserId.Should().Be(DatabaseSeeder.TestUserId1);
            }
        }

        [Theory]
        [InlineData("api/poems/like/{0}", "NoContent", true)]
        [InlineData("api/poems/like/nonexistent-id", "NotFound", false)]
        public async Task LikePoem_ReturnsExpectedStatusCode(string urlTemplate, string expectedStatus, bool checkIsLiked)
        {
            // Arrange
            var url = string.Format(urlTemplate, DatabaseSeeder.TestPoemId1);
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());

            // Act
            var response = await Client.PostAsync(url, null);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (checkIsLiked)
            {
                // Check if the poem is liked
                var likedResponse = await Client.GetAsync($"api/poems/liked/{DatabaseSeeder.TestPoemId1}");
                likedResponse.IsSuccessStatusCode.Should().BeTrue();

                var isLiked = await likedResponse.Content.ReadFromJsonAsync<bool>();
                isLiked.Should().BeTrue("the poem should be liked after the like action");
            }
        }

        [Theory]
        [InlineData("api/poems/view/{0}", "NoContent")]
        [InlineData("api/poems/view/nonexistent-id", "NoContent")] // Should not cause error
        public async Task IncrementViewCount_ReturnsNoContent(string urlTemplate, string expectedStatus)
        {
            // Arrange
            var url = string.Format(urlTemplate, DatabaseSeeder.TestPoemId1);

            // Act
            var response = await Client.PostAsync(url, null);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));
        }
    }
}