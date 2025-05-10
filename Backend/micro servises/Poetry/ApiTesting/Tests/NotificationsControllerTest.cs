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
    public class NotificationsControllerTest
    {
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly NotificationsController _controller;
        private readonly Guid _currentUserId;

        public NotificationsControllerTest()
        {
            _mockNotificationService = new Mock<INotificationService>();
            _controller = new NotificationsController(_mockNotificationService.Object);
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
        public async Task GetNotifications_ShouldReturnAllNotifications()
        {
            // Arrange
            var notifications = new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = Guid.NewGuid(),
                    UserId = _currentUserId,
                    PoemId = Guid.NewGuid(),
                    PoemTitle = "Test Poem",
                    Message = "Someone liked your poem",
                    Type = "NewLike",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new NotificationDto
                {
                    Id = Guid.NewGuid(),
                    UserId = _currentUserId,
                    PoemId = Guid.NewGuid(),
                    PoemTitle = "Another Poem",
                    Message = "Someone commented on your poem",
                    Type = "NewComment",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };

            _mockNotificationService.Setup(s => s.GetUserNotificationsAsync(_currentUserId, false))
                .ReturnsAsync(notifications);

            // Act
            var result = await _controller.GetNotifications(false);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedNotifications = Assert.IsAssignableFrom<IEnumerable<NotificationDto>>(okResult.Value);
            Assert.Equal(2, returnedNotifications.Count());
            Assert.Contains(returnedNotifications, n => n.Type == "NewLike");
            Assert.Contains(returnedNotifications, n => n.Type == "NewComment");
        }

        [Fact]
        public async Task GetNotifications_WithUnreadOnly_ShouldFilterUnread()
        {
            // Arrange
            var notifications = new List<NotificationDto>
            {
                new NotificationDto
                {
                    Id = Guid.NewGuid(),
                    UserId = _currentUserId,
                    PoemId = Guid.NewGuid(),
                    PoemTitle = "Test Poem",
                    Message = "Someone liked your poem",
                    Type = "NewLike",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                }
            };

            _mockNotificationService.Setup(s => s.GetUserNotificationsAsync(_currentUserId, true))
                .ReturnsAsync(notifications);

            // Act
            var result = await _controller.GetNotifications(true);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedNotifications = Assert.IsAssignableFrom<IEnumerable<NotificationDto>>(okResult.Value);
            Assert.Single(returnedNotifications);
            Assert.Equal("NewLike", returnedNotifications.First().Type);

            // Verify service was called with correct parameters
            _mockNotificationService.Verify(s => s.GetUserNotificationsAsync(_currentUserId, true), Times.Once);
        }

        [Fact]
        public async Task MarkAsRead_WhenSuccessful_ShouldReturnNoContent()
        {
            // Arrange
            var notificationId = Guid.NewGuid();

            _mockNotificationService.Setup(s => s.MarkNotificationAsReadAsync(notificationId, _currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.MarkAsRead(notificationId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_WhenNotificationNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var notificationId = Guid.NewGuid();

            _mockNotificationService.Setup(s => s.MarkNotificationAsReadAsync(notificationId, _currentUserId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.MarkAsRead(notificationId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task MarkAllAsRead_ShouldReturnNoContent()
        {
            // Arrange
            _mockNotificationService.Setup(s => s.MarkAllNotificationsAsReadAsync(_currentUserId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.MarkAllAsRead();

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify service was called
            _mockNotificationService.Verify(s => s.MarkAllNotificationsAsReadAsync(_currentUserId), Times.Once);
        }
    }
}
