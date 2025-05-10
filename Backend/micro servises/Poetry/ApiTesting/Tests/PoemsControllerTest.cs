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
    public class PoemsControllerTest
    {
        private readonly Mock<IPoemService> _mockPoemService;
        private readonly Mock<ICommentService> _mockCommentService;
        private readonly Mock<ILikeService> _mockLikeService;
        private readonly PoemsController _controller;
        private readonly Guid _currentUserId;
        private readonly string _userName;

        public PoemsControllerTest()
        {
            _mockPoemService = new Mock<IPoemService>();
            _mockCommentService = new Mock<ICommentService>();
            _mockLikeService = new Mock<ILikeService>();
            _controller = new PoemsController(
                _mockPoemService.Object,
                _mockCommentService.Object,
                _mockLikeService.Object);

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
            _controller.HttpContext.User = user;
            _controller.HttpContext.User.AddIdentity(identity);
        }

        [Fact]
        public async Task GetPoems_ShouldReturnSearchResults()
        {
            // Arrange
            var searchDto = new PoemSearchDto
            {
                SearchTerm = "nature",
                PageNumber = 1,
                PageSize = 10
            };

            var poemDtos = new List<PoemDto>
            {
                new PoemDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Nature's Beauty",
                    AuthorId = Guid.NewGuid(),
                    AuthorName = "Jane Doe"
                },
                new PoemDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Mountain Views",
                    AuthorId = Guid.NewGuid(),
                    AuthorName = "John Smith"
                }
            };

            var paginatedResult = new PaginatedResult<PoemDto>
            {
                Items = poemDtos,
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 2,
                TotalPages = 1
            };

            _mockPoemService.Setup(s => s.SearchPoemsAsync(It.IsAny<PoemSearchDto>()))
                .ReturnsAsync(paginatedResult);

            // Act
            var result = await _controller.GetPoems(searchDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResult = Assert.IsType<PaginatedResult<PoemDto>>(okResult.Value);
            Assert.Equal(2, returnedResult.Items.Count);
            Assert.Equal(1, returnedResult.PageNumber);
            Assert.Equal(10, returnedResult.PageSize);
            Assert.Equal(2, returnedResult.TotalCount);
            Assert.False(returnedResult.HasNext);
        }

        [Fact]
        public async Task GetPoemsByAuthor_ShouldReturnAuthorPoems()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var poems = new List<PoemDto>
            {
                new PoemDto
                {
                    Id = Guid.NewGuid(),
                    Title = "First Poem",
                    AuthorId = authorId,
                    AuthorName = "Author Name"
                },
                new PoemDto
                {
                    Id = Guid.NewGuid(),
                    Title = "Second Poem",
                    AuthorId = authorId,
                    AuthorName = "Author Name"
                }
            };

            _mockPoemService.Setup(s => s.GetPoemsByAuthorIdAsync(authorId))
                .ReturnsAsync(poems);

            // Act
            var result = await _controller.GetPoemsByAuthor(authorId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPoems = Assert.IsAssignableFrom<IEnumerable<PoemDto>>(okResult.Value);
            Assert.Equal(2, returnedPoems.Count());
            Assert.Equal("First Poem", returnedPoems.First().Title);
            Assert.Equal("Second Poem", returnedPoems.Last().Title);
        }

        [Fact]
        public async Task GetPoemContent_WhenPoemExists_ShouldReturnContent()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var content = "This is the full content of the poem...";

            _mockPoemService.Setup(s => s.GetPoemContentAsync(poemId))
                .ReturnsAsync(content);

            // Act
            var result = await _controller.GetPoemContent(poemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedContent = Assert.IsType<string>(okResult.Value);
            Assert.Equal(content, returnedContent);
        }

        [Fact]
        public async Task GetPoemContent_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.GetPoemContentAsync(poemId))
                .ReturnsAsync((string)null);

            // Act
            var result = await _controller.GetPoemContent(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPoemDetailsAsync_WhenPoemExists_ShouldReturnDetails()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var poemDetails = new PoemDetailsDto
            {
                Id = poemId,
                Title = "Test Poem",
                Content = "Full poem content",
                AuthorId = Guid.NewGuid(),
                AuthorName = "Poet Name",
                IsPublished = true,
                Comments = new List<CommentDto>
                {
                    new CommentDto
                    {
                        Id = Guid.NewGuid(),
                        Text = "Great poem!"
                    }
                },
                IsLikedByCurrentUser = true
            };

            _mockPoemService.Setup(s => s.GetPoemDetailsAsync(poemId, It.IsAny<Guid?>()))
                .ReturnsAsync(poemDetails);

            // Act
            var result = await _controller.GetPoemDetailsAsync(poemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDetails = Assert.IsType<PoemDetailsDto>(okResult.Value);
            Assert.Equal(poemId, returnedDetails.Id);
            Assert.Equal("Test Poem", returnedDetails.Title);
            Assert.Single(returnedDetails.Comments);
            Assert.True(returnedDetails.IsLikedByCurrentUser);

            // Verify IncrementViewCountAsync was called
            _mockPoemService.Verify(s => s.IncrementViewCountAsync(poemId), Times.Once);
        }

        [Fact]
        public async Task GetPoemDetailsAsync_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.GetPoemDetailsAsync(poemId, It.IsAny<Guid?>()))
                .ReturnsAsync((PoemDetailsDto)null);

            // Act
            var result = await _controller.GetPoemDetailsAsync(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPoemComments_ShouldReturnComments()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var pageNumber = 1;
            var pageSize = 10;

            var comments = new List<CommentDto>
            {
                new CommentDto
                {
                    Id = Guid.NewGuid(),
                    PoemId = poemId,
                    UserId = Guid.NewGuid(),
                    UserName = "Commenter 1",
                    Text = "Excellent poem!",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new CommentDto
                {
                    Id = Guid.NewGuid(),
                    PoemId = poemId,
                    UserId = Guid.NewGuid(),
                    UserName = "Commenter 2",
                    Text = "Beautiful imagery",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            _mockCommentService.Setup(s => s.GetCommentsByPoemIdAsync(poemId, pageNumber, pageSize))
                .ReturnsAsync(comments);

            // Act
            var result = await _controller.GetPoemComments(poemId, pageNumber, pageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedComments = Assert.IsAssignableFrom<IEnumerable<CommentDto>>(okResult.Value);
            Assert.Equal(2, returnedComments.Count());
        }

        [Fact]
        public async Task AddComment_WhenSuccessful_ShouldReturnCreatedComment()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var commentText = "This is my comment on the poem";

            var createdComment = new CommentDto
            {
                Id = Guid.NewGuid(),
                PoemId = poemId,
                UserId = _currentUserId,
                UserName = _userName,
                Text = commentText,
                CreatedAt = DateTime.UtcNow
            };

            _mockCommentService.Setup(s => s.AddCommentAsync(It.IsAny<CreateCommentDto>()))
                .ReturnsAsync(createdComment);

            // Act
            var result = await _controller.AddComment(poemId, commentText);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedComment = Assert.IsType<CommentDto>(createdAtActionResult.Value);

            Assert.Equal(poemId, returnedComment.PoemId);
            Assert.Equal(_currentUserId, returnedComment.UserId);
            Assert.Equal(_userName, returnedComment.UserName);
            Assert.Equal(commentText, returnedComment.Text);

            // Verify comment DTO was created correctly
            _mockCommentService.Verify(s => s.AddCommentAsync(
                It.Is<CreateCommentDto>(dto =>
                    dto.PoemId == poemId &&
                    dto.UserId == _currentUserId &&
                    dto.UserName == _userName &&
                    dto.Text == commentText)),
                Times.Once);
        }

        [Fact]
        public async Task AddComment_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var commentText = "This is my comment";

            _mockCommentService.Setup(s => s.AddCommentAsync(It.IsAny<CreateCommentDto>()))
                .ThrowsAsync(new KeyNotFoundException("Poem not found"));

            // Act
            var result = await _controller.AddComment(poemId, commentText);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteComment_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var commentId = Guid.NewGuid();

            _mockCommentService.Setup(s => s.DeleteCommentAsync(commentId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteComment(commentId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteComment_WhenNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var commentId = Guid.NewGuid();

            _mockCommentService.Setup(s => s.DeleteCommentAsync(commentId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteComment(commentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteComment_WhenUnauthorized_ShouldReturnForbid()
        {
            // Arrange
            var commentId = Guid.NewGuid();

            _mockCommentService.Setup(s => s.DeleteCommentAsync(commentId, _currentUserId))
                .ThrowsAsync(new UnauthorizedAccessException("Only the comment author or poem author can delete this comment"));

            // Act
            var result = await _controller.DeleteComment(commentId);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task LikePoem_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockLikeService.Setup(s => s.LikePoemAsync(poemId, _currentUserId, _userName))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.LikePoem(poemId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify like service was called with correct parameters
            _mockLikeService.Verify(s => s.LikePoemAsync(poemId, _currentUserId, _userName), Times.Once);
        }

        [Fact]
        public async Task LikePoem_WhenPoemNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockLikeService.Setup(s => s.LikePoemAsync(poemId, _currentUserId, _userName))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.LikePoem(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UnlikePoem_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockLikeService.Setup(s => s.UnlikePoemAsync(poemId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.UnlikePoem(poemId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify unlike service was called
            _mockLikeService.Verify(s => s.UnlikePoemAsync(poemId, _currentUserId), Times.Once);
        }

        [Fact]
        public async Task UnlikePoem_WhenPoemNotLiked_ShouldReturnNotFound()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockLikeService.Setup(s => s.UnlikePoemAsync(poemId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.UnlikePoem(poemId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task IsPoemLiked_ShouldReturnLikeStatus()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var isLiked = true;

            _mockLikeService.Setup(s => s.IsPoemLikedByUserAsync(poemId, _currentUserId))
                .ReturnsAsync(isLiked);

            // Act
            var result = await _controller.IsPoemLiked(poemId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var likeStatus = Assert.IsType<bool>(okResult.Value);
            Assert.True(likeStatus);
        }

        [Fact]
        public async Task IncrementViewCount_ShouldIncrementAndReturnNoContent()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            _mockPoemService.Setup(s => s.IncrementViewCountAsync(poemId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.IncrementViewCount(poemId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify view count was incremented
            _mockPoemService.Verify(s => s.IncrementViewCountAsync(poemId), Times.Once);
        }
    }
}
