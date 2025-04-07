using BLL.User.DTOs;
using BLL.User.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.User.Controllers
{
    [ApiController]
    [Route("api/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            var result = await _identityService.RegisterUserAsync(registerDto);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Registration successful. Please verify your email." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _identityService.LoginAsync(loginDto);
            if (result == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // Set refresh token cookie
            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                accessToken = result.AccessToken,
                userId = result.UserId,
                userName = result.UserName,
                expiresIn = result.ExpiresIn
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Invalid token" });

            var result = await _identityService.RefreshTokenAsync(refreshToken);
            if (result == null)
                return Unauthorized(new { message = "Invalid refresh token" });

            // Set new refresh token cookie
            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(new
            {
                accessToken = result.AccessToken,
                userId = result.UserId,
                userName = result.UserName,
                expiresIn = result.ExpiresIn
            });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string userId, string token)
        {
            var result = await _identityService.VerifyEmailAsync(userId, token);
            return result
                ? Ok(new { message = "Email verified successfully" })
                : BadRequest(new { message = "Invalid verification token" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            await _identityService.ForgotPasswordAsync(forgotPasswordDto.Email);
            return Ok(new { message = "If your email exists in our system, you will receive password reset instructions." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _identityService.ResetPasswordAsync(resetPasswordDto);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Password reset successful." });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _identityService.LogoutAsync(refreshToken);
                // Remove the refresh token cookie
                Response.Cookies.Delete("refreshToken");
            }

            return Ok(new { message = "Logout successful." });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }

}
