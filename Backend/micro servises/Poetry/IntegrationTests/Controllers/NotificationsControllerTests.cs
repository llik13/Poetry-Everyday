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
    public class NotificationsControllerTests : IntegrationTestBase
    {
        public NotificationsControllerTests(CustomWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData("api/notifications")]
        [InlineData("api/notifications?unreadOnly=true")]
        [InlineData("api/notifications?unreadOnly=false")]
        public async Task GetNotifications_ReturnsSuccessStatusCode(string endpoint)
        {
            // Arrange
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());

            // Act
            var response = await Client.GetAsync(endpoint);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();

            var notifications = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
            notifications.Should().NotBeNull();

            if (endpoint.Contains("unreadOnly=true") && notifications!.Any())
            {
                foreach (var notification in notifications)
                {
                    notification.IsRead.Should().BeFalse("when requesting unread only, all notifications should be unread");
                }
            }
        }

        [Theory]
        [InlineData("read/{0}", "NoContent", true)] // Mark notification as read
        [InlineData("read/nonexistent-id", "NotFound", true)] // Non-existent notification
        public async Task MarkAsRead_ReturnsExpectedStatusCode(string urlTemplate, string expectedStatus, bool useTestUser1)
        {
            // Arrange
            var userId = useTestUser1 ? DatabaseSeeder.TestUserId1 : DatabaseSeeder.TestUserId2;
            AuthenticateClient(userId.ToString());

            // First get a notification ID
            var getResponse = await Client.GetAsync("api/notifications?unreadOnly=true");
            getResponse.IsSuccessStatusCode.Should().BeTrue();

            var notifications = await getResponse.Content.ReadFromJsonAsync<List<NotificationDto>>();
            notifications.Should().NotBeNull();

            string url;
            if (urlTemplate.Contains("{0}") && notifications!.Any())
            {
                var notificationId = notifications.First().Id;
                url = string.Format(urlTemplate, notificationId);
            }
            else
            {
                url = urlTemplate.Replace("{0}", "nonexistent-id");
            }

            // Act
            var response = await Client.PutAsync($"api/notifications/{url}", null);

            // Assert
            response.StatusCode.Should().Be(Enum.Parse<HttpStatusCode>(expectedStatus));

            if (response.IsSuccessStatusCode && urlTemplate.Contains("{0}") && notifications!.Any())
            {
                // Verify the notification is now marked as read
                var updatedResponse = await Client.GetAsync("api/notifications");
                updatedResponse.IsSuccessStatusCode.Should().BeTrue();

                var updatedNotifications = await updatedResponse.Content.ReadFromJsonAsync<List<NotificationDto>>();
                updatedNotifications.Should().NotBeNull();

                var notificationId = notifications!.First().Id;
                var updatedNotification = updatedNotifications!.FirstOrDefault(n => n.Id == notificationId);
                updatedNotification.Should().NotBeNull();
                updatedNotification!.IsRead.Should().BeTrue("notification should be marked as read");
            }
        }

        [Fact]
        public async Task MarkAllAsRead_ReturnsNoContent()
        {
            // Arrange
            AuthenticateClient(DatabaseSeeder.TestUserId1.ToString());

            // Act
            var response = await Client.PutAsync("api/notifications/read-all", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify all notifications are marked as read
            var getResponse = await Client.GetAsync("api/notifications?unreadOnly=true");
            getResponse.IsSuccessStatusCode.Should().BeTrue();

            var notifications = await getResponse.Content.ReadFromJsonAsync<List<NotificationDto>>();
            notifications.Should().NotBeNull();
            notifications.Should().BeEmpty("all notifications should be marked as read");
        }
    }
}