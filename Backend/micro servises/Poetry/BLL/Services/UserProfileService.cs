using BLL.User.DTOs;
using BLL.User.Interfaces;
using Contract.Change;
using DAL.User.Entities;
using DAL.User.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Identity;


namespace BLL.User.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserActivityRepository _activityRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UserProfileService(
            UserManager<ApplicationUser> userManager,
            IUserActivityRepository activityRepository,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _activityRepository = activityRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            return new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                Biography = user.Biography,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin,
                NotificationSettings = new NotificationSettingsDto
                {
                    EmailNotifications = user.NotificationPreferences.EmailNotifications,
                    ForumReplies = user.NotificationPreferences.ForumReplies,
                    PoemComments = user.NotificationPreferences.PoemComments,
                    PoemLikes = user.NotificationPreferences.PoemLikes,
                    Newsletter = user.NotificationPreferences.Newsletter
                }
            };
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto updateProfileDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            // Update biography
            user.Biography = updateProfileDto.Biography;

            // Check if username is being updated
            string oldUsername = user.UserName;
            bool usernameChanged = false;

            // Update username if provided
            if (!string.IsNullOrEmpty(updateProfileDto.UserName) && user.UserName != updateProfileDto.UserName)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, updateProfileDto.UserName);
                if (!setUsernameResult.Succeeded)
                    return false;

                usernameChanged = true;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Log activity
                await _activityRepository.AddActivityAsync(new UserActivity
                {
                    UserId = userId,
                    Type = ActivityType.UpdateProfile,
                    Description = "User updated their profile"
                });

                // Publish username changed event if username was updated
                if (usernameChanged)
                {
                    await _publishEndpoint.Publish<UserNameChangedEvent>(new
                    {
                        UserId = userId,
                        OldUserName = oldUsername,
                        NewUserName = updateProfileDto.UserName,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            return result.Succeeded;
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword
            );
        }



        public async Task<List<ActivityDto>> GetUserActivityAsync(string userId, int page = 1, int pageSize = 10)
        {
            var activities = await _activityRepository.GetUserActivitiesAsync(userId, page, pageSize);

            return activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                Type = a.Type.ToString(),
                Description = a.Description,
                RelatedId = a.RelatedId,
                RelatedTitle = a.RelatedTitle,
                CreatedAt = a.CreatedAt
            }).ToList();
        }

        public async Task<bool> UpdateNotificationSettingsAsync(string userId, NotificationSettingsDto settingsDto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            user.NotificationPreferences = new NotificationPreferences
            {
                EmailNotifications = settingsDto.EmailNotifications,
                ForumReplies = settingsDto.ForumReplies,
                PoemComments = settingsDto.PoemComments,
                PoemLikes = settingsDto.PoemLikes,
                Newsletter = settingsDto.Newsletter
            };

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Log activity
                await _activityRepository.AddActivityAsync(new UserActivity
                {
                    UserId = userId,
                    Type = ActivityType.UpdateProfile,
                    Description = "User updated notification settings"
                });
            }

            return result.Succeeded;
        }

    }
}