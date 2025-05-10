using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Presentation.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiTesting.Tests
{
    public class UserPoemsControllerTest
    {
        private readonly Mock<IPoemService> _mockPoemService;
        private readonly UserPoemsController _controller;
        private readonly Guid _currentUserId;
        private readonly string _userName;

        public UserPoemsControllerTest()
        {
            _mockPoemService = new Mock<IPoemService>();
            _controller = new UserPoemsController(_mockPoemService.Object);

            _currentUserId = Guid.NewGuid();
            _userName = "Test User";

            // Setup controller context with user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _currentUserId.ToString()),
                new Claim(ClaimTypes.Name, _userName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetMyPoems_ShouldReturnCurrentUserPoems()
        {
            // Arrange
            var poems = new List<PoemDto>
            {
                new PoemDto
                {
                    Id = Guid.NewGuid(),
                    Title = "My First Poem",
                    AuthorId = _currentUserId,
                    AuthorName = _userName
                },
                new PoemDto
                {
                    Id = Guid.NewGuid(),
                    Title = "My Second Poem",
                    AuthorId = _currentUserId,
                    AuthorName = _userName
                }
            };

            _mockPoemService.Setup(s => s.GetPoemsByAuthorIdAsync(_currentUserId))
                .ReturnsAsync(poems);

            // Act
            var result = await _controller.GetMyPoems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPoems = Assert.IsAssignableFrom<IEnumerable<PoemDto>>(okResult.Value);
            Assert.Equal(2, returnedPoems.Count());
            Assert.Equal("My First Poem", returnedPoems.First().Title);
            Assert.Equal(_currentUserId, returnedPoems.First().AuthorId);
        }

        [Fact]
        public async Task GetMyDrafts_ShouldReturnUnpublishedPoems()
        {
            // Arrange
            var draftPoems = new List<PoemDto>
            {
                new PoemDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Draft Poem",
                    AuthorId = _currentUserId,
                    AuthorName = _userName,
                    IsPublished = false
                }
            };

            var paginatedResult = new PaginatedResult<PoemDto>
            {
                Items = draftPoems,
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1,
                TotalPages = 1
            };

            _mockPoemService.Setup(s => s.SearchPoemsAsync(It.IsAny<PoemSearchDto>()))
                .ReturnsAsync(paginatedResult);

            // Act
            var result = await _controller.GetMyDrafts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PaginatedResult<PoemDto>>(okResult.Value);
            Assert.Single(returnedResult.Items);
            Assert.Equal("Draft Poem", returnedResult.Items.First().Title);

            // Verify search was called with correct parameters
            _mockPoemService.Verify(s => s.SearchPoemsAsync(
                It.Is<PoemSearchDto>(dto =>
                    dto.AuthorId == _currentUserId &&
                    dto.IsPublished == false)),
                Times.Once);
        }

        [Fact]
        public async Task GetPoem_WhenPoemExists_ShouldReturnPoemDetails()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var poem = new PoemDetailsDto
            {
                Id = poemId,
                Title = "Test Poem",
                Content = "Poem content",
                AuthorId = _currentUserId,
                AuthorName = _userName,
                IsPublished = true
            };

            _mockPoemService.Setup(s => s.GetPoemDetailsAsync(poemId, _currentUserId))
                .ReturnsAsync(poem);

            // Act
            var result = await _controller.GetPoem(poemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPoem = Assert.IsType<PoemDetailsDto>(okResult.Value);
            Assert.Equal(poemId, returnedPoem.Id);
            Assert.Equal("Test Poem", returnedPoem.Title);
        }

        [Fact]
        public async Task GetPoem_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.GetPoemDetailsAsync(poemId, _currentUserId))
                .ReturnsAsync((PoemDetailsDto)null);

            // Act
            var result = await _controller.GetPoem(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreatePoem_ShouldCreateAndReturnPoem()
        {
            // Arrange
            var createDto = new CreatePoemDto
            {
                Title = "New Poem",
                Content = "Poem content",
                Excerpt = "Poem excerpt",
                Tags = new List<string> { "nature", "love" },
                Categories = new List<string> { "lyric" }
            };

            var createdPoem = new PoemDto
            {
                Id = Guid.NewGuid(),
                Title = createDto.Title,
                Content = createDto.Content,
                Excerpt = createDto.Excerpt,
                AuthorId = _currentUserId,
                AuthorName = _userName,
                IsPublished = false,
                Tags = createDto.Tags,
                Categories = createDto.Categories
            };

            _mockPoemService.Setup(s => s.CreatePoemAsync(It.IsAny<CreatePoemDto>()))
                .ReturnsAsync(createdPoem);

            // Act
            var result = await _controller.CreatePoem(createDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedPoem = Assert.IsType<PoemDto>(createdAtActionResult.Value);

            Assert.Equal(createdPoem.Id, returnedPoem.Id);
            Assert.Equal("New Poem", returnedPoem.Title);
            Assert.Equal(_currentUserId, returnedPoem.AuthorId);

            // Verify author details were set correctly
            _mockPoemService.Verify(s => s.CreatePoemAsync(
                It.Is<CreatePoemDto>(dto =>
                    dto.AuthorId == _currentUserId &&
                    dto.AuthorName == _userName)),
                Times.Once);
        }

        [Fact]
        public async Task UpdatePoem_WhenSuccessful_ShouldReturnUpdatedPoem()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var updateDto = new UpdatePoemDto
            {
                Id = poemId,
                Title = "Updated Title",
                Content = "Updated content",
                Excerpt = "Updated excerpt",
                IsPublished = true,
                Tags = new List<string> { "updated" },
                Categories = new List<string> { "modern" },
                ChangeNotes = "Updated the poem"
            };

            var updatedPoem = new PoemDto
            {
                Id = poemId,
                Title = updateDto.Title,
                Content = updateDto.Content,
                Excerpt = updateDto.Excerpt,
                AuthorId = _currentUserId,
                AuthorName = _userName,
                IsPublished = updateDto.IsPublished,
                Tags = updateDto.Tags,
                Categories = updateDto.Categories
            };

            _mockPoemService.Setup(s => s.UpdatePoemAsync(updateDto, _currentUserId))
                .ReturnsAsync(updatedPoem);

            // Act
            var result = await _controller.UpdatePoem(poemId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPoem = Assert.IsType<PoemDto>(okResult.Value);

            Assert.Equal(poemId, returnedPoem.Id);
            Assert.Equal("Updated Title", returnedPoem.Title);
            Assert.True(returnedPoem.IsPublished);
        }

        [Fact]
        public async Task UpdatePoem_WhenIdMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var updateDto = new UpdatePoemDto
            {
                Id = Guid.NewGuid(), // Different ID
                Title = "Updated Title"
            };

            // Act
            var result = await _controller.UpdatePoem(poemId, updateDto);

            // Assert
            Assert.IsType<BadRequestResult>(result.Result);
        }

        [Fact]
        public async Task UpdatePoem_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var updateDto = new UpdatePoemDto
            {
                Id = poemId,
                Title = "Updated Title"
            };

            _mockPoemService.Setup(s => s.UpdatePoemAsync(updateDto, _currentUserId))
                .ReturnsAsync((PoemDto)null);

            // Act
            var result = await _controller.UpdatePoem(poemId, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdatePoem_WhenUnauthorized_ShouldReturnForbid()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var updateDto = new UpdatePoemDto
            {
                Id = poemId,
                Title = "Updated Title"
            };

            _mockPoemService.Setup(s => s.UpdatePoemAsync(updateDto, _currentUserId))
                .ThrowsAsync(new UnauthorizedAccessException("Only the author can update this poem"));

            // Act
            var result = await _controller.UpdatePoem(poemId, updateDto);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task UnpublishPoem_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.UnpublishPoemAsync(poemId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UnpublishPoem(poemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UnpublishPoem_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.UnpublishPoemAsync(poemId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UnpublishPoem(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UnpublishPoem_WhenUnauthorized_ShouldReturnForbid()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.UnpublishPoemAsync(poemId, _currentUserId))
                .ThrowsAsync(new UnauthorizedAccessException("Only the author can unpublish this poem"));

            // Act
            var result = await _controller.UnpublishPoem(poemId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task DeletePoem_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.DeletePoemAsync(poemId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeletePoem(poemId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePoem_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.DeletePoemAsync(poemId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeletePoem(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeletePoem_WhenUnauthorized_ShouldReturnForbid()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.DeletePoemAsync(poemId, _currentUserId))
                .ThrowsAsync(new UnauthorizedAccessException("Only the author can delete this poem"));

            // Act
            var result = await _controller.DeletePoem(poemId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task PublishPoem_WhenSuccessful_ShouldReturnUpdatedPoem()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            var poem = new PoemDto
            {
                Id = poemId,
                Title = "Draft Poem",
                Content = "Poem content",
                Excerpt = "Poem excerpt",
                AuthorId = _currentUserId,
                IsPublished = false,
                Tags = new List<string> { "draft" },
                Categories = new List<string> { "personal" }
            };

            var updatedPoem = new PoemDto
            {
                Id = poemId,
                Title = "Draft Poem",
                Content = "Poem content",
                Excerpt = "Poem excerpt",
                AuthorId = _currentUserId,
                IsPublished = true, // Now published
                Tags = new List<string> { "draft" },
                Categories = new List<string> { "personal" }
            };

            _mockPoemService.Setup(s => s.GetPoemByIdAsync(poemId))
                .ReturnsAsync(poem);

            _mockPoemService.Setup(s => s.UpdatePoemAsync(It.IsAny<UpdatePoemDto>(), _currentUserId))
                .ReturnsAsync(updatedPoem);

            // Act
            var result = await _controller.PublishPoem(poemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPoem = Assert.IsType<PoemDto>(okResult.Value);

            Assert.Equal(poemId, returnedPoem.Id);
            Assert.True(returnedPoem.IsPublished);

            // Verify the update was called with correct parameters
            _mockPoemService.Verify(s => s.UpdatePoemAsync(
                It.Is<UpdatePoemDto>(dto =>
                    dto.Id == poemId &&
                    dto.IsPublished == true &&
                    dto.ChangeNotes == "Published poem"),
                _currentUserId),
                Times.Once);
        }

        [Fact]
        public async Task PublishPoem_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.GetPoemByIdAsync(poemId))
                .ReturnsAsync((PoemDto)null);

            // Act
            var result = await _controller.PublishPoem(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PublishPoem_WhenNotAuthor_ShouldReturnForbid()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var poem = new PoemDto
            {
                Id = poemId,
                Title = "Draft Poem",
                AuthorId = otherUserId, // Different from current user
                IsPublished = false
            };

            _mockPoemService.Setup(s => s.GetPoemByIdAsync(poemId))
                .ReturnsAsync(poem);

            // Act
            var result = await _controller.PublishPoem(poemId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
