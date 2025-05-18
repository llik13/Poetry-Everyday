using BLL.User.DTOs;
using BLL.User.Interfaces;
using BLL.User.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.User.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/profile")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _profileService;
        private readonly IProfileImageService _profileImageService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            IUserProfileService profileService,
            IProfileImageService profileImageService,
            ILogger<UserProfileController> logger)
        {
            _profileService = profileService;
            _profileImageService = profileImageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _profileService.GetUserProfileAsync(userId);

            if (profile == null)
                return NotFound();

            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _profileService.UpdateUserProfileAsync(userId, updateProfileDto);

            return result
                ? Ok(new { message = "Profile updated successfully" })
                : BadRequest(new { message = "Failed to update profile" });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _profileService.ChangePasswordAsync(userId, changePasswordDto);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image provided" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            using (var stream = image.OpenReadStream())
            {
                var imageUrl = await _profileImageService.SaveProfileImageAsync(userId, stream);
                return Ok(new { imageUrl });
            }
        }

        [HttpGet("image/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfileImage(string userId)
        {
            try
            {
                var (imageData, contentType) = await _profileImageService.GetProfileImageAsync(userId);

                if (imageData == null || contentType == null)
                {
                    // Логирование для диагностики
                    _logger.LogWarning($"Image not found for user {userId}");

                    // Возвращаем стандартное изображение или 404
                    return NotFound();
                }

                // Логирование для диагностики
                _logger.LogInformation($"Returning image for user {userId}, content type: {contentType}, size: {imageData.Length} bytes");

                return File(imageData, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving image for user {userId}");
                return StatusCode(500, "Internal server error retrieving image");
            }
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetUserActivity([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activities = await _profileService.GetUserActivityAsync(userId, page, pageSize);

            return Ok(activities);
        }

        [HttpPut("notification-settings")]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsDto settingsDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _profileService.UpdateNotificationSettingsAsync(userId, settingsDto);

            return result
                ? Ok(new { message = "Notification settings updated" })
                : BadRequest(new { message = "Failed to update notification settings" });
        }

    }

}
