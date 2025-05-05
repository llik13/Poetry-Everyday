using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using BusinessLogic.Services;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Moq;
using static System.Net.Mime.MediaTypeNames;

namespace PoetryServiceTest.Tests
{
    public class CommentServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CommentService _commentService;

        public CommentServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _commentService = new CommentService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task AddCommentAsync_ShouldAddCommentAndUpdateStatistics()
        {
            // Arrange
            var commentDto = new CreateCommentDto
            {
                PoemId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UserName = "TestUser",
                Text = "This is a test comment"
            };

            var poem = new Poem
            {
                Id = commentDto.PoemId,
                Title = "Test Poem",
                AuthorId = Guid.NewGuid(),
                Statistics = new PoemStatistics
                {
                    CommentCount = 0
                }
            };

            var poemRepository = new Mock<IPoemRepository>();
            poemRepository.Setup(repo => repo.GetPoemWithDetailsAsync(commentDto.PoemId))
                .ReturnsAsync(poem);

            var commentRepository = new Mock<ICommentRepository>();
            commentRepository.Setup(repo => repo.AddAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            var notificationRepository = new Mock<IPoemNotificationRepository>();
            notificationRepository.Setup(repo => repo.AddAsync(It.IsAny<PoemNotification>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _commentService.AddCommentAsync(commentDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentDto.PoemId, result.PoemId);
            Assert.Equal(commentDto.UserId, result.UserId);
            Assert.Equal(commentDto.UserName, result.UserName);
            Assert.Equal(commentDto.Text, result.Text);

            // Verify comment was added
            commentRepository.Verify(repo =>
                repo.AddAsync(It.Is<Comment>(c =>
                    c.PoemId == commentDto.PoemId &&
                    c.UserId == commentDto.UserId &&
                    c.UserName == commentDto.UserName &&
                    c.Text == commentDto.Text)),
                Times.Once);

            // Verify statistics were updated
            poemRepository.Verify(repo =>
                repo.Update(It.Is<Poem>(p =>
                    p.Statistics.CommentCount == 1)),
                Times.Once);

            // Verify notification was created
            notificationRepository.Verify(repo =>
                repo.AddAsync(It.Is<PoemNotification>(n =>
                    n.UserId == poem.AuthorId &&
                    n.PoemId == poem.Id &&
                    n.Type == NotificationType.NewComment &&
                    n.Message.Contains(commentDto.UserName) &&
                    n.Message.Contains(poem.Title))),
                Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task AddCommentAsync_WhenPoemNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var commentDto = new CreateCommentDto
            {
                PoemId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UserName = "TestUser",
                Text = "This is a test comment"
            };

            var poemRepository = new Mock<IPoemRepository>();
            poemRepository.Setup(repo => repo.GetPoemWithDetailsAsync(commentDto.PoemId))
                .ReturnsAsync((Poem)null);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepository.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _commentService.AddCommentAsync(commentDto));
        }

        [Fact]
        public async Task AddCommentAsync_WhenUserIsAuthor_ShouldNotCreateNotification()
        {
            // Arrange
            var authorId = Guid.NewGuid();

            var commentDto = new CreateCommentDto
            {
                PoemId = Guid.NewGuid(),
                UserId = authorId, // Same as author
                UserName = "AuthorUser",
                Text = "This is a comment by the author"
            };

            var poem = new Poem
            {
                Id = commentDto.PoemId,
                Title = "Test Poem",
                AuthorId = authorId, // Same as commenter
                Statistics = new PoemStatistics
                {
                    CommentCount = 0
                }
            };

            var poemRepository = new Mock<IPoemRepository>();
            poemRepository.Setup(repo => repo.GetPoemWithDetailsAsync(commentDto.PoemId))
                .ReturnsAsync(poem);

            var commentRepository = new Mock<ICommentRepository>();
            commentRepository.Setup(repo => repo.AddAsync(It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            var notificationRepository = new Mock<IPoemNotificationRepository>();

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepository.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _commentService.AddCommentAsync(commentDto);

            // Assert
            // Verify comment was added and stats updated
            commentRepository.Verify(repo => repo.AddAsync(It.IsAny<Comment>()), Times.Once);
            poemRepository.Verify(repo => repo.Update(It.IsAny<Poem>()), Times.Once);

            // Verify no notification was created (because author commented on their own poem)
            notificationRepository.Verify(repo => repo.AddAsync(It.IsAny<PoemNotification>()), Times.Never);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenIsUserId_ShouldDeleteComment()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var comment = new Comment
            {
                Id = commentId,
                PoemId = poemId,
                UserId = userId
            };
            var poem = new Poem
            {
                Id = poemId,
                AuthorId = Guid.NewGuid(),
                Statistics = new PoemStatistics { CommentCount = 1 }
            };
            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.GetByIdAsync(commentId)).ReturnsAsync(comment);
            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(comment.PoemId)).ReturnsAsync(poem);
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId)).ReturnsAsync(poem);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _commentService.DeleteCommentAsync(commentId, userId);

            // Assert
            Assert.True(result);
            commentRepo.Verify(repo => repo.SoftDelete(comment), Times.Once);
            poemRepo.Verify(repo => repo.Update(It.Is<Poem>(p => p.Statistics.CommentCount == 0)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenIsPoemAuthor_ShouldDeleteComment()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var comment = new Comment
            {
                Id = commentId,
                PoemId = poemId,
                UserId = userId // Different from authorId
            };
            var poem = new Poem
            {
                Id = poemId,
                AuthorId = authorId,
                Statistics = new PoemStatistics { CommentCount = 1 }
            };
            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.GetByIdAsync(commentId)).ReturnsAsync(comment);
            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(comment.PoemId)).ReturnsAsync(poem);
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId)).ReturnsAsync(poem);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _commentService.DeleteCommentAsync(commentId, authorId);

            // Assert
            Assert.True(result);
            commentRepo.Verify(repo => repo.SoftDelete(comment), Times.Once);
            poemRepo.Verify(repo => repo.Update(It.Is<Poem>(p => p.Statistics.CommentCount == 0)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenNotAuthorized_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid(); // Neither comment author nor poem author
            var comment = new Comment
            {
                Id = commentId,
                PoemId = poemId,
                UserId = userId
            };
            var poem = new Poem
            {
                Id = poemId,
                AuthorId = authorId,
                Statistics = new PoemStatistics { CommentCount = 1 }
            };
            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.GetByIdAsync(commentId)).ReturnsAsync(comment);
            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(comment.PoemId)).ReturnsAsync(poem);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _commentService.DeleteCommentAsync(commentId, currentUserId));
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenCommentNotFound_ShouldReturnFalse()
        {
            // Arrange
            var commentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.GetByIdAsync(commentId)).ReturnsAsync((Comment)null);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);

            // Act
            var result = await _commentService.DeleteCommentAsync(commentId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenPoemNotFound_ShouldReturnFalse()
        {
            //Arrange
            var commentId = Guid.NewGuid();
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var comment = new Comment
            {
                Id = commentId,
                PoemId = poemId,
                UserId = userId
            };
            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.GetByIdAsync(commentId)).ReturnsAsync(comment);
            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(poemId)).ReturnsAsync((Poem)null);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);

            //Act
            var result = await _commentService.DeleteCommentAsync(userId, commentId);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetCommentsByPoemIdAsync_WhenPoemHasComments_ShouldReturnMatchingComments()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var pageNumber = 1;
            var pageSize = 5;

            // Create test data
            var commentsFromRepository = new List<Comment>
    {
        new Comment
        {
            Id = Guid.NewGuid(),
            PoemId = poemId,
            UserId = Guid.NewGuid(),
            UserName = "Alice",
            Text = "Beautiful imagery in this poem!",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        },
        new Comment
        {
            Id = Guid.NewGuid(),
            PoemId = poemId,
            UserId = Guid.NewGuid(),
            UserName = "Bob",
            Text = "The rhythm flows perfectly.",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        }
    };

            // Set up the repository stub
            var commentRepoStub = new Mock<ICommentRepository>();
            commentRepoStub
                .Setup(repo => repo.GetCommentsByPoemIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(commentsFromRepository);

            var unitOfWorkStub = new Mock<IUnitOfWork>();
            unitOfWorkStub.Setup(uow => uow.Comments).Returns(commentRepoStub.Object);

            var commentService = new CommentService(unitOfWorkStub.Object);

            // Act
            var result = await commentService.GetCommentsByPoemIdAsync(poemId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentsFromRepository.Count, result.Count());

            // Check the mapping is correct by comparing the first comment
            var firstResult = result.First();
            var firstExpected = commentsFromRepository.First();

            Assert.Equal(firstExpected.Id, firstResult.Id);
            Assert.Equal(firstExpected.PoemId, firstResult.PoemId);
            Assert.Equal(firstExpected.UserId, firstResult.UserId);
            Assert.Equal(firstExpected.UserName, firstResult.UserName);
            Assert.Equal(firstExpected.Text, firstResult.Text);
            Assert.Equal(firstExpected.CreatedAt, firstResult.CreatedAt);
        }

        [Fact]
        public async Task GetCommentsByPoemIdAsync_ShouldHandlePagination()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var pageNumber = 2;
            var pageSize = 3;

            // Create test data for the second page
            var commentsFromRepository = new List<Comment>
    {
        new Comment
        {
            Id = Guid.NewGuid(),
            PoemId = poemId,
            UserId = Guid.NewGuid(),
            UserName = "Charlie",
            Text = "This reminded me of Frost's work",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        },
        new Comment
        {
            Id = Guid.NewGuid(),
            PoemId = poemId,
            UserId = Guid.NewGuid(),
            UserName = "Diana",
            Text = "The metaphors are striking",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        }
    };

            // Set up the repository stub - notice we use It.IsAny for the parameters
            var commentRepoStub = new Mock<ICommentRepository>();
            commentRepoStub
                .Setup(repo => repo.GetCommentsByPoemIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(commentsFromRepository);

            var unitOfWorkStub = new Mock<IUnitOfWork>();
            unitOfWorkStub.Setup(uow => uow.Comments).Returns(commentRepoStub.Object);

            var commentService = new CommentService(unitOfWorkStub.Object);

            // Act
            var result = await commentService.GetCommentsByPoemIdAsync(poemId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentsFromRepository.Count, result.Count());

            // Verify the DTOs are mapped correctly
            var resultList = result.ToList();
            for (int i = 0; i < commentsFromRepository.Count; i++)
            {
                Assert.Equal(commentsFromRepository[i].Id, resultList[i].Id);
                Assert.Equal(commentsFromRepository[i].PoemId, resultList[i].PoemId);
                Assert.Equal(commentsFromRepository[i].UserName, resultList[i].UserName);
                Assert.Equal(commentsFromRepository[i].Text, resultList[i].Text);
                Assert.Equal(commentsFromRepository[i].CreatedAt, resultList[i].CreatedAt);
            }
        }

        [Fact]
        public async Task GetCommentsByPoemIdAsync_WhenPoemHasNoComments_ShouldReturnEmptyCollection()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var pageNumber = 1;
            var pageSize = 5;

            // Set up the repository stub to return an empty collection
            var emptyCommentsList = new List<Comment>();

            var commentRepoStub = new Mock<ICommentRepository>();
            commentRepoStub
                .Setup(repo => repo.GetCommentsByPoemIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(emptyCommentsList);

            var unitOfWorkStub = new Mock<IUnitOfWork>();
            unitOfWorkStub.Setup(uow => uow.Comments).Returns(commentRepoStub.Object);

            var commentService = new CommentService(unitOfWorkStub.Object);

            // Act
            var result = await commentService.GetCommentsByPoemIdAsync(poemId, pageNumber, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
