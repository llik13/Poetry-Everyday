using BusinessLogic.Consumer;
using Contract.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PoetryServiceTest.Tests
{
    public class UserNameChangedConsumerTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<UserNameChangedConsumer>> _mockLogger;
        private readonly UserNameChangedConsumer _consumer;

        public UserNameChangedConsumerTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<UserNameChangedConsumer>>();
            _consumer = new UserNameChangedConsumer(_mockUnitOfWork.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Consume_ShouldUpdateAuthorNameInPoemsAndComments()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userGuid = Guid.Parse(userId);
            var oldName = "Old Author Name";
            var newName = "New Author Name";

            // Setup event message
            var mockMessage = new Mock<IUserNameChangedEvent>();
            mockMessage.Setup(m => m.UserId).Returns(userId);
            mockMessage.Setup(m => m.OldUserName).Returns(oldName);
            mockMessage.Setup(m => m.NewUserName).Returns(newName);

            // Setup consume context
            var mockConsumeContext = new Mock<ConsumeContext<IUserNameChangedEvent>>();
            mockConsumeContext.Setup(c => c.Message).Returns(mockMessage.Object);

            // Setup poems by author
            var poems = new List<Poem>
            {
                new Poem
                {
                    Id = Guid.NewGuid(),
                    Title = "First Poem",
                    AuthorId = userGuid,
                    AuthorName = oldName
                },
                new Poem
                {
                    Id = Guid.NewGuid(),
                    Title = "Second Poem",
                    AuthorId = userGuid,
                    AuthorName = oldName
                }
            };

            // Setup comments by user
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    PoemId = Guid.NewGuid(),
                    UserId = userGuid,
                    UserName = oldName,
                    Text = "Great poem!"
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    PoemId = Guid.NewGuid(),
                    UserId = userGuid,
                    UserName = oldName,
                    Text = "I loved this one!"
                }
            };

            // Mock repositories
            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemsByAuthorIdAsync(userGuid))
                .ReturnsAsync(poems);

            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Comment, bool>>>()))
                .ReturnsAsync(comments);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert
            // Verify all poems were updated with new author name
            foreach (var poem in poems)
            {
                Assert.Equal(newName, poem.AuthorName);
                poemRepo.Verify(repo => repo.Update(poem), Times.Once);
            }

            // Verify all comments were updated with new user name
            foreach (var comment in comments)
            {
                Assert.Equal(newName, comment.UserName);
                commentRepo.Verify(repo => repo.Update(comment), Times.Once);
            }

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task Consume_WithInvalidGuid_ShouldLogErrorAndNotUpdateAnything()
        {
            // Arrange
            var userId = "invalid-guid"; // Invalid GUID format
            var oldName = "Old Author Name";
            var newName = "New Author Name";

            // Setup event message
            var mockMessage = new Mock<IUserNameChangedEvent>();
            mockMessage.Setup(m => m.UserId).Returns(userId);
            mockMessage.Setup(m => m.OldUserName).Returns(oldName);
            mockMessage.Setup(m => m.NewUserName).Returns(newName);

            // Setup consume context
            var mockConsumeContext = new Mock<ConsumeContext<IUserNameChangedEvent>>();
            mockConsumeContext.Setup(c => c.Message).Returns(mockMessage.Object);

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert
            // Verify no poems or comments were updated
            _mockUnitOfWork.Verify(uow => uow.Poems, Times.Never);
            _mockUnitOfWork.Verify(uow => uow.Comments, Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);

            // Verify error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Consume_WithNoPoems_ShouldOnlyUpdateComments()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userGuid = Guid.Parse(userId);
            var oldName = "Old Author Name";
            var newName = "New Author Name";

            // Setup event message
            var mockMessage = new Mock<IUserNameChangedEvent>();
            mockMessage.Setup(m => m.UserId).Returns(userId);
            mockMessage.Setup(m => m.OldUserName).Returns(oldName);
            mockMessage.Setup(m => m.NewUserName).Returns(newName);

            // Setup consume context
            var mockConsumeContext = new Mock<ConsumeContext<IUserNameChangedEvent>>();
            mockConsumeContext.Setup(c => c.Message).Returns(mockMessage.Object);

            // Setup empty poems list (no poems by this author)
            var poems = new List<Poem>();

            // Setup comments by user
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    PoemId = Guid.NewGuid(),
                    UserId = userGuid,
                    UserName = oldName,
                    Text = "Great poem!"
                }
            };

            // Mock repositories
            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemsByAuthorIdAsync(userGuid))
                .ReturnsAsync(poems);

            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Comment, bool>>>()))
                .ReturnsAsync(comments);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            await _consumer.Consume(mockConsumeContext.Object);

            // Assert
            // Verify no poems were updated
            poemRepo.Verify(repo => repo.Update(It.IsAny<Poem>()), Times.Never);

            // Verify comments were updated
            Assert.Equal(newName, comments.First().UserName);
            commentRepo.Verify(repo => repo.Update(It.IsAny<Comment>()), Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task Consume_WithException_ShouldRethrowException()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var userGuid = Guid.Parse(userId);
            var oldName = "Old Author Name";
            var newName = "New Author Name";

            // Setup event message
            var mockMessage = new Mock<IUserNameChangedEvent>();
            mockMessage.Setup(m => m.UserId).Returns(userId);
            mockMessage.Setup(m => m.OldUserName).Returns(oldName);
            mockMessage.Setup(m => m.NewUserName).Returns(newName);

            // Setup consume context
            var mockConsumeContext = new Mock<ConsumeContext<IUserNameChangedEvent>>();
            mockConsumeContext.Setup(c => c.Message).Returns(mockMessage.Object);

            // Setup poems by author
            var poems = new List<Poem>
    {
        new Poem
        {
            Id = Guid.NewGuid(),
            Title = "First Poem",
            AuthorId = userGuid,
            AuthorName = oldName
        }
    };

            // Setup comments by user - adding this to avoid null reference
            var comments = new List<Comment>(); // Empty list but not null

            // Mock repositories
            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemsByAuthorIdAsync(userGuid))
                .ReturnsAsync(poems);

            var commentRepo = new Mock<ICommentRepository>();
            commentRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Comment, bool>>>()))
                .ReturnsAsync(comments); // Return empty list instead of null

            var exception = new InvalidOperationException("Test exception");

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Comments).Returns(commentRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ThrowsAsync(exception);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _consumer.Consume(mockConsumeContext.Object));

            Assert.Equal("Test exception", thrownException.Message);

            // Verify error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
