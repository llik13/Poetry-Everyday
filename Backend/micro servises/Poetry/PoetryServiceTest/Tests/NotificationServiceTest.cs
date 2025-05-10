using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
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
    public class NotificationServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPoemService> _mockPoemService;
        private readonly NotificationService _notificationService;

        public NotificationServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPoemService = new Mock<IPoemService>();
            _notificationService = new NotificationService(_mockUnitOfWork.Object, _mockPoemService.Object);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnAllNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var poemId1 = Guid.NewGuid();
            var poemId2 = Guid.NewGuid();

            var notifications = new List<PoemNotification>
            {
                new PoemNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PoemId = poemId1,
                    Message = "New like on your poem",
                    Type = NotificationType.NewLike,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new PoemNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PoemId = poemId2,
                    Message = "New comment on your poem",
                    Type = NotificationType.NewComment,
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            var poem1 = new PoemDto { Id = poemId1, Title = "Poem 1" };
            var poem2 = new PoemDto { Id = poemId2, Title = "Poem 2" };

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PoemNotification, bool>>>()))
                .ReturnsAsync(notifications);

            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);

            _mockPoemService.Setup(service => service.GetPoemByIdAsync(poemId1))
                .ReturnsAsync(poem1);
            _mockPoemService.Setup(service => service.GetPoemByIdAsync(poemId2))
                .ReturnsAsync(poem2);

            // Act
            var result = await _notificationService.GetUserNotificationsAsync(userId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            var resultList = result.ToList();
            Assert.Equal(notifications[0].Id, resultList[0].Id);
            Assert.Equal(notifications[0].Message, resultList[0].Message);
            Assert.Equal(poem1.Title, resultList[0].PoemTitle);
            Assert.Equal("NewLike", resultList[0].Type);
            Assert.False(resultList[0].IsRead);

            Assert.Equal(notifications[1].Id, resultList[1].Id);
            Assert.Equal(notifications[1].Message, resultList[1].Message);
            Assert.Equal(poem2.Title, resultList[1].PoemTitle);
            Assert.Equal("NewComment", resultList[1].Type);
            Assert.True(resultList[1].IsRead);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_WithUnreadOnly_ShouldReturnOnlyUnreadNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            var notifications = new List<PoemNotification>
            {
                new PoemNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PoemId = poemId,
                    Message = "New like on your poem",
                    Type = NotificationType.NewLike,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var poem = new PoemDto { Id = poemId, Title = "Poem Title" };

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PoemNotification, bool>>>()))
                .ReturnsAsync(notifications);

            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);

            _mockPoemService.Setup(service => service.GetPoemByIdAsync(poemId))
                .ReturnsAsync(poem);

            // Act
            var result = await _notificationService.GetUserNotificationsAsync(userId, true);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(notifications[0].Id, result.First().Id);
            Assert.Equal("Poem Title", result.First().PoemTitle);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_WhenPoemNotFound_ShouldReturnUnknownPoemTitle()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            var notifications = new List<PoemNotification>
            {
                new PoemNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    PoemId = poemId,
                    Message = "New like on your poem",
                    Type = NotificationType.NewLike,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PoemNotification, bool>>>()))
                .ReturnsAsync(notifications);

            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);

            _mockPoemService.Setup(service => service.GetPoemByIdAsync(poemId))
                .ReturnsAsync((PoemDto)null);

            // Act
            var result = await _notificationService.GetUserNotificationsAsync(userId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Unknown Poem", result.First().PoemTitle);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_ShouldMarkNotificationAsRead()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var notification = new PoemNotification
            {
                Id = notificationId,
                UserId = userId,
                IsRead = false
            };

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PoemNotification, bool>>>()))
                .ReturnsAsync(new List<PoemNotification> { notification });

            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);

            // Assert
            Assert.True(result);
            Assert.True(notification.IsRead);

            // Verify notification was updated
            notificationRepo.Verify(repo => repo.Update(notification), Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task MarkNotificationAsReadAsync_WhenNotificationNotFound_ShouldReturnFalse()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PoemNotification, bool>>>()))
                .ReturnsAsync(new List<PoemNotification>());

            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);

            // Act
            var result = await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);

            // Assert
            Assert.False(result);

            // Verify no changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
        }

        [Fact]
        public async Task MarkAllNotificationsAsReadAsync_ShouldMarkAllUnreadNotificationsAsRead()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var notifications = new List<PoemNotification>
            {
                new PoemNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsRead = false
                },
                new PoemNotification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    IsRead = false
                }
            };

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PoemNotification, bool>>>()))
                .ReturnsAsync(notifications);

            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

            // Assert
            Assert.True(result);
            Assert.All(notifications, n => Assert.True(n.IsRead));

            // Verify all notifications were updated
            notificationRepo.Verify(repo => repo.Update(It.IsAny<PoemNotification>()), Times.Exactly(2));

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task MarkAllNotificationsAsReadAsync_WhenNoUnreadNotifications_ShouldReturnTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<PoemNotification, bool>>>()))
                .ReturnsAsync(new List<PoemNotification>());

            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId);

            // Assert
            Assert.True(result);

            // Verify changes were saved (even though there were no notifications to update)
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
}
