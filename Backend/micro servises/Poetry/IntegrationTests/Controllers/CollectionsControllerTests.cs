using BusinessLogic.DTOs;
using FluentAssertions;
using Poetry.IntegrationTests.Extensions;
using Poetry.IntegrationTests.TestData;
using Presentation;
using System.Net;
using Xunit;

namespace Poetry.IntegrationTests.Controllers
{
    public class CollectionsControllerTests : IntegrationTestBase
    {
        public CollectionsControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetUserCollections_ReturnsSuccessStatusCode()
        {
            // Arrange
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());
            
            // Act
            var response = await Client.GetAsync("api/collections");

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            
            var collections = await response.Content.ReadFromJsonAsync<List<CollectionDto>>();
            collections.Should().NotBeNull();
            collections.Should().Contain(c => c.Id == DatabaseSeeder.TestCollectionId);
        }

        [Theory]
        [InlineData("{0}", "OK", true)] // Own collection
        [InlineData("nonexistent-id", "NotFound", true)]
        [InlineData("{0}", "OK", false)] // Other user's collection, but it's public
        public async Task GetCollection_ReturnsExpectedStatusCode(string idTemplate, string expectedStatus, bool useTestUser1)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());
            
            var id = idTemplate.Contains("{0}") 
                ? string.Format(idTemplate, DatabaseSeeder.TestCollectionId)
                : idTemplate;

            // Act
            var response = await Client.GetAsync($"api/collections/{id}");

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));
            
            if (response.IsSuccessStatusCode)
            {
                var collection = await response.Content.ReadFromJsonAsync<CollectionDetailsDto>();
                collection.Should().NotBeNull();
                collection!.Id.Should().Be(DatabaseSeeder.TestCollectionId);
                collection.Poems.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task CreateCollection_ReturnsCreatedResponse()
        {
            // Arrange
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());
            
            var newCollection = new CreateCollectionDto
            {
                Name = "New Test Collection",
                Description = "This is a test collection created during integration testing.",
                IsPublic = true
                // UserId will be set by the controller
            };

            // Act
            var response = await Client.PostAsJsonAsync("api/collections", newCollection);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createdCollection = await response.Content.ReadFromJsonAsync<CollectionDto>();
            createdCollection.Should().NotBeNull();
            createdCollection!.Name.Should().Be(newCollection.Name);
            createdCollection.Description.Should().Be(newCollection.Description);
            
            if (newCollection.IsPublic)
                createdCollection.IsPublic.Should().BeTrue();
            else
                createdCollection.IsPublic.Should().BeFalse();
                
            createdCollection.UserId.Should().Be(DatabaseSeeder.TestUserId1);
        }

        [Theory]
        [InlineData("{0}", "{1}", true, "NoContent")] // Add poem to own collection
        [InlineData("{0}", "{1}", false, "Forbidden")] // Add poem to another user's collection
        [InlineData("nonexistent-id", "{1}", true, "NotFound")] // Non-existent collection
        [InlineData("{0}", "nonexistent-id", true, "NotFound")] // Non-existent poem
        public async Task AddPoemToCollection_ReturnsExpectedStatusCode(
            string collectionIdTemplate, string poemIdTemplate, bool useTestUser1, string expectedStatus)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());
            
            var collectionId = collectionIdTemplate.Contains("{0}") 
                ? string.Format(collectionIdTemplate, DatabaseSeeder.TestCollectionId)
                : collectionIdTemplate;
                
            var poemId = poemIdTemplate.Contains("{1}") 
                ? string.Format(poemIdTemplate, DatabaseSeeder.TestPoemId1)
                : poemIdTemplate;

            // Act
            var response = await Client.PostAsync($"api/collections/{collectionId}/poems/{poemId}", null);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));
            
            if (response.IsSuccessStatusCode)
            {
                // Verify the poem was added to the collection
                var collectionResponse = await Client.GetAsync($"api/collections/{collectionId}");
                collectionResponse.IsSuccessStatusCode.Should().BeTrue();
                
                var collection = await collectionResponse.Content.ReadFromJsonAsync<CollectionDetailsDto>();
                collection.Should().NotBeNull();
                
                // Check if the poem exists in the collection
                var poemGuid = Guid.Parse(poemId);
                collection!.Poems.Should().Contain(p => p.Id == poemGuid);
            }
        }

        [Theory]
        [InlineData("{0}", "{1}", true, "NoContent")] // Remove poem from own collection
        [InlineData("{0}", "{1}", false, "Forbidden")] // Remove poem from another user's collection
        [InlineData("nonexistent-id", "{1}", true, "NotFound")] // Non-existent collection
        [InlineData("{0}", "nonexistent-id", true, "NotFound")] // Non-existent poem
        public async Task RemovePoemFromCollection_ReturnsExpectedStatusCode(
            string collectionIdTemplate, string poemIdTemplate, bool useTestUser1, string expectedStatus)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());
            
            var collectionId = collectionIdTemplate.Contains("{0}") 
                ? string.Format(collectionIdTemplate, DatabaseSeeder.TestCollectionId)
                : collectionIdTemplate;
                
            var poemId = poemIdTemplate.Contains("{1}") 
                ? string.Format(poemIdTemplate, DatabaseSeeder.TestPoemId2) // Use TestPoemId2 which is in the collection
                : poemIdTemplate;

            // Act
            var response = await Client.DeleteAsync($"api/collections/{collectionId}/poems/{poemId}");

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));
            
            if (response.IsSuccessStatusCode)
            {
                // Verify the poem was removed from the collection
                var collectionResponse = await Client.GetAsync($"api/collections/{collectionId}");
                collectionResponse.IsSuccessStatusCode.Should().BeTrue();
                
                var collection = await collectionResponse.Content.ReadFromJsonAsync<CollectionDetailsDto>();
                collection.Should().NotBeNull();
                
                // Check that the poem is no longer in the collection
                var poemGuid = Guid.Parse(poemId);
                collection!.Poems.Should().NotContain(p => p.Id == poemGuid);
            }
        }

        [Theory]
        [InlineData("{0}", true, "NoContent")] // Delete own collection
        [InlineData("{0}", false, "Forbidden")] // Delete another user's collection
        [InlineData("nonexistent-id", true, "NotFound")] // Non-existent collection
        public async Task DeleteCollection_ReturnsExpectedStatusCode(
            string collectionIdTemplate, bool useTestUser1, string expectedStatus)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());
            
            var collectionId = collectionIdTemplate.Contains("{0}") 
                ? string.Format(collectionIdTemplate, DatabaseSeeder.TestCollectionId)
                : collectionIdTemplate;

            // Act
            var response = await Client.DeleteAsync($"api/collections/{collectionId}");

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));
            
            if (response.IsSuccessStatusCode)
            {
                // Verify the collection was deleted
                var collectionResponse = await Client.GetAsync($"api/collections/{collectionId}");
                collectionResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}