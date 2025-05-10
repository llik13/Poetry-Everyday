using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Presentation.Controllers;
using System.Security.Claims;


namespace ApiTesting.Tests
{
    public class CollectionsControllerTest
    {
        private readonly Mock<ICollectionService> _mockCollectionService;
        private readonly CollectionsController _controller;
        private readonly Guid _currentUserId;

        public CollectionsControllerTest()
        {
            _mockCollectionService = new Mock<ICollectionService>();
            _controller = new CollectionsController(_mockCollectionService.Object);
            _currentUserId = Guid.NewGuid();

            // Setup controller context with user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _currentUserId.ToString()),
                new Claim(ClaimTypes.Name, "Test User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetUserCollections_ShouldReturnCollections()
        {
            // Arrange
            var collections = new List<CollectionDto>
            {
                new CollectionDto
                {
                    Id = Guid.NewGuid(),
                    Name = "My Favorites",
                    Description = "My favorite poems",
                    UserId = _currentUserId,
                    IsPublic = true,
                    PoemCount = 5
                },
                new CollectionDto
                {
                    Id = Guid.NewGuid(),
                    Name = "Nature Poems",
                    Description = "Collection of nature poems",
                    UserId = _currentUserId,
                    IsPublic = false,
                    PoemCount = 3
                }
            };

            _mockCollectionService.Setup(s => s.GetUserCollectionsAsync(_currentUserId))
                .ReturnsAsync(collections);

            // Act
            var result = await _controller.GetUserCollections();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCollections = Assert.IsAssignableFrom<IEnumerable<CollectionDto>>(okResult.Value);
            Assert.Equal(2, returnedCollections.Count());
            Assert.Equal(collections[0].Id, returnedCollections.First().Id);
            Assert.Equal(collections[1].Id, returnedCollections.Last().Id);
        }

        [Fact]
        public async Task GetCollection_WhenCollectionExists_ShouldReturnCollection()
        {
            // Arrange
            var collectionId = Guid.NewGuid();
            var collection = new CollectionDetailsDto
            {
                Id = collectionId,
                Name = "My Favorites",
                Description = "My favorite poems",
                UserId = _currentUserId,
                IsPublic = true,
                PoemCount = 5,
                Poems = new List<PoemDto>
                {
                    new PoemDto
                    {
                        Id = Guid.NewGuid(),
                        Title = "Sample Poem",
                        AuthorId = Guid.NewGuid(),
                        AuthorName = "Test Author"
                    }
                }
            };

            _mockCollectionService.Setup(s => s.GetCollectionAsync(collectionId, _currentUserId))
                .ReturnsAsync(collection);

            // Act
            var result = await _controller.GetCollection(collectionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedCollection = Assert.IsType<CollectionDetailsDto>(okResult.Value);
            Assert.Equal(collectionId, returnedCollection.Id);
            Assert.Equal("My Favorites", returnedCollection.Name);
            Assert.Equal(1, returnedCollection.Poems.Count);
        }

        [Fact]
        public async Task GetCollection_WhenCollectionNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var collectionId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.GetCollectionAsync(collectionId, _currentUserId))
                .ReturnsAsync((CollectionDetailsDto)null);

            // Act
            var result = await _controller.GetCollection(collectionId);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCollection_WhenUnauthorized_ShouldReturnForbid()
        {
            // Arrange
            var collectionId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.GetCollectionAsync(collectionId, _currentUserId))
                .ThrowsAsync(new UnauthorizedAccessException("You don't have access to this collection"));

            // Act
            var result = await _controller.GetCollection(collectionId);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task CreateCollection_ShouldReturnCreatedCollection()
        {
            // Arrange
            var createDto = new CreateCollectionDto
            {
                Name = "New Collection",
                Description = "A new collection of poems",
                IsPublic = true
            };

            var createdCollection = new CollectionDto
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                UserId = _currentUserId,
                IsPublic = createDto.IsPublic,
                PoemCount = 0
            };

            _mockCollectionService.Setup(s => s.CreateCollectionAsync(It.IsAny<CreateCollectionDto>()))
                .ReturnsAsync(createdCollection);

            // Act
            var result = await _controller.CreateCollection(createDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedCollection = Assert.IsType<CollectionDto>(createdAtActionResult.Value);

            Assert.Equal(createdCollection.Id, returnedCollection.Id);
            Assert.Equal("New Collection", returnedCollection.Name);
            Assert.Equal(_currentUserId, returnedCollection.UserId);

            // Verify the UserId is set to current user
            _mockCollectionService.Verify(s => s.CreateCollectionAsync(
                It.Is<CreateCollectionDto>(dto => dto.UserId == _currentUserId)),
                Times.Once);
        }

        [Fact]
        public async Task AddPoemToCollection_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var collectionId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.AddPoemToCollectionAsync(collectionId, poemId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddPoemToCollection(collectionId, poemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task AddPoemToCollection_WhenNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var collectionId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.AddPoemToCollectionAsync(collectionId, poemId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.AddPoemToCollection(collectionId, poemId);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
        }

        [Fact]
        public async Task AddPoemToCollection_WhenUnauthorized_ShouldReturnForbid()
        {
            // Arrange
            var collectionId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.AddPoemToCollectionAsync(collectionId, poemId, _currentUserId))
                .ThrowsAsync(new UnauthorizedAccessException("Only the collection owner can add poems"));

            // Act
            var result = await _controller.AddPoemToCollection(collectionId, poemId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task RemovePoemFromCollection_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var collectionId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.RemovePoemFromCollectionAsync(collectionId, poemId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RemovePoemFromCollection(collectionId, poemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemovePoemFromCollection_WhenNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var collectionId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.RemovePoemFromCollectionAsync(collectionId, poemId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.RemovePoemFromCollection(collectionId, poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCollection_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var collectionId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.DeleteCollectionAsync(collectionId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCollection(collectionId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteCollection_WhenNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var collectionId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.DeleteCollectionAsync(collectionId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCollection(collectionId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCollection_WhenUnauthorized_ShouldReturnForbid()
        {
            // Arrange
            var collectionId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.DeleteCollectionAsync(collectionId, _currentUserId))
                .ThrowsAsync(new UnauthorizedAccessException("Only the collection owner can delete this collection"));

            // Act
            var result = await _controller.DeleteCollection(collectionId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
