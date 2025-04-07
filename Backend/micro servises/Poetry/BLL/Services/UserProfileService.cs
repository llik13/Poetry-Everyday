using BLL.User.DTOs;
using BLL.User.Interfaces;
using DAL.User.Entities;
using DAL.User.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserActivityRepository _activityRepository;
        private readonly IFileStorageService _fileStorageService;

        public UserProfileService(
            UserManager<ApplicationUser> userManager,
            IUserActivityRepository activityRepository,
            IFileStorageService fileStorageService)
        {
            _userManager = userManager;
            _activityRepository = activityRepository;
            _fileStorageService = fileStorageService;
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

            // Update username if provided
            if (!string.IsNullOrEmpty(updateProfileDto.UserName) && user.UserName != updateProfileDto.UserName)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, updateProfileDto.UserName);
                if (!setUsernameResult.Succeeded)
                    return false;
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

        public async Task<string> UploadProfileImageAsync(string userId, Stream imageStream)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            // Delete old image if exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                await _fileStorageService.DeleteFileAsync(user.ProfileImageUrl);
            }

            // Save new image
            string fileName = $"profile-images/{userId}/{System.Guid.NewGuid()}.jpg";
            var imageUrl = await _fileStorageService.SaveFileAsync(fileName, imageStream);

            // Update user
            user.ProfileImageUrl = imageUrl;
            await _userManager.UpdateAsync(user);

            // Log activity
            await _activityRepository.AddActivityAsync(new UserActivity
            {
                UserId = userId,
                Type = ActivityType.UpdateProfile,
                Description = "User updated their profile picture"
            });

            return imageUrl;
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
