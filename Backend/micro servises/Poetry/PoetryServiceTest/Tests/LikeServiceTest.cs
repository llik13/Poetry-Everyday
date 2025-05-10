using BusinessLogic.Services;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PoetryServiceTest.Tests
{
    public class LikeServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly LikeService _likeService;

        public LikeServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _likeService = new LikeService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task LikePoemAsync_ShouldAddLikeAndUpdateStatistics()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "TestUser";

            var poem = new Poem
            {
                Id = poemId,
                Title = "Test Poem",
                AuthorId = Guid.NewGuid(),
                Statistics = new PoemStatistics
                {
                    LikeCount = 0
                }
            };

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(new List<Like>());

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(poem);

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.AddAsync(It.IsAny<PoemNotification>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _likeService.LikePoemAsync(poemId, userId, userName);

            // Assert
            Assert.True(result);

            // Verify like was added
            likeRepo.Verify(repo => repo.AddAsync(It.Is<Like>(l =>
                l.PoemId == poemId && l.UserId == userId)), Times.Once);

            // Verify statistics were updated
            poemRepo.Verify(repo => repo.Update(It.Is<Poem>(p =>
                p.Statistics.LikeCount == 1)), Times.Once);

            // Verify notification was created
            notificationRepo.Verify(repo => repo.AddAsync(It.Is<PoemNotification>(n =>
                n.UserId == poem.AuthorId &&
                n.PoemId == poem.Id &&
                n.Type == NotificationType.NewLike &&
                n.Message.Contains(userName) &&
                n.Message.Contains(poem.Title))), Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task LikePoemAsync_WhenAlreadyLiked_ShouldReturnTrueWithoutAdding()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "TestUser";

            var existingLike = new Like
            {
                Id = Guid.NewGuid(),
                PoemId = poemId,
                UserId = userId
            };

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(new List<Like> { existingLike });

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(new Poem { Id = poemId });

            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act
            var result = await _likeService.LikePoemAsync(poemId, userId, userName);

            // Assert
            Assert.True(result);

            // Verify no like was added
            likeRepo.Verify(repo => repo.AddAsync(It.IsAny<Like>()), Times.Never);

            // Verify no statistics were updated
            poemRepo.Verify(repo => repo.Update(It.IsAny<Poem>()), Times.Never);

            // Verify no notification was created
            _mockUnitOfWork.Verify(uow => uow.Notifications, Times.Never);

            // Verify no changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task LikePoemAsync_WhenPoemNotFound_ShouldReturnFalse()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userName = "TestUser";

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync((Poem)null);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act
            var result = await _likeService.LikePoemAsync(poemId, userId, userName);

            // Assert
            Assert.False(result);

            // Verify no changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task LikePoemAsync_WhenUserIsAuthor_ShouldNotCreateNotification()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var poemId = Guid.NewGuid();
            var userName = "AuthorUser";

            var poem = new Poem
            {
                Id = poemId,
                Title = "Test Poem",
                AuthorId = authorId, // Same as the userId being passed
                Statistics = new PoemStatistics
                {
                    LikeCount = 0
                }
            };

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(new List<Like>());

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(poem);

            var notificationRepo = new Mock<IPoemNotificationRepository>();

            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _likeService.LikePoemAsync(poemId, authorId, userName);

            // Assert
            Assert.True(result);

            // Verify like was added and stats updated
            likeRepo.Verify(repo => repo.AddAsync(It.IsAny<Like>()), Times.Once);
            poemRepo.Verify(repo => repo.Update(It.IsAny<Poem>()), Times.Once);

            // Verify no notification was created (because author liked their own poem)
            notificationRepo.Verify(repo => repo.AddAsync(It.IsAny<PoemNotification>()), Times.Never);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UnlikePoemAsync_ShouldRemoveLikeAndUpdateStatistics()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingLike = new Like
            {
                Id = Guid.NewGuid(),
                PoemId = poemId,
                UserId = userId
            };

            var poem = new Poem
            {
                Id = poemId,
                Statistics = new PoemStatistics
                {
                    LikeCount = 1
                }
            };

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(new List<Like> { existingLike });

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(poem);

            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _likeService.UnlikePoemAsync(poemId, userId);

            // Assert
            Assert.True(result);

            // Verify like was removed
            likeRepo.Verify(repo => repo.Remove(existingLike), Times.Once);

            // Verify statistics were updated
            poemRepo.Verify(repo => repo.Update(It.Is<Poem>(p =>
                p.Statistics.LikeCount == 0)), Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UnlikePoemAsync_WhenNotLiked_ShouldReturnFalse()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(new List<Like>());

            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);

            // Act
            var result = await _likeService.UnlikePoemAsync(poemId, userId);

            // Assert
            Assert.False(result);

            // Verify no changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task IsPoemLikedByUserAsync_WhenLiked_ShouldReturnTrue()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var existingLike = new Like
            {
                Id = Guid.NewGuid(),
                PoemId = poemId,
                UserId = userId
            };

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(new List<Like> { existingLike });

            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);

            // Act
            var result = await _likeService.IsPoemLikedByUserAsync(poemId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsPoemLikedByUserAsync_WhenNotLiked_ShouldReturnFalse()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(new List<Like>());

            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);

            // Act
            var result = await _likeService.IsPoemLikedByUserAsync(poemId, userId);

            // Assert
            Assert.False(result);
        }
    }
}
