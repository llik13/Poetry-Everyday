using BLL.User.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Interfaces
{
    public interface IUserProfileService
    {
        Task<UserProfileDto> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto updateProfileDto);
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<List<ActivityDto>> GetUserActivityAsync(string userId, int page = 1, int pageSize = 10);
        Task<bool> UpdateNotificationSettingsAsync(string userId, NotificationSettingsDto settingsDto);
    }

}
