using BLL.User.DTOs;
using BLL.User.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.User.Controllers
{
    [ApiController]
    [Route("api/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly IConfiguration _configuration;

        public IdentityController(IIdentityService identityService, IConfiguration configuration)
        {
            _identityService = identityService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            _logger.LogInformation("Registration attempt: Username={Username}, Email={Email}",
                registerDto.UserName, registerDto.Email);

            if (registerDto == null)
            {
                _logger.LogWarning("Registration failed: RegisterDto is null");
                return BadRequest(new { message = "Registration data is missing" });
            }

            if (string.IsNullOrEmpty(registerDto.UserName))
            {
                _logger.LogWarning("Registration failed: Username is missing");
                return BadRequest(new { message = "Username is required" });
            }

            if (string.IsNullOrEmpty(registerDto.Email))
            {
                _logger.LogWarning("Registration failed: Email is missing");
                return BadRequest(new { message = "Email is required" });
            }

            if (string.IsNullOrEmpty(registerDto.Password))
            {
                _logger.LogWarning("Registration failed: Password is missing");
                return BadRequest(new { message = "Password is required" });
            }

            var result = await _identityService.RegisterUserAsync(registerDto);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Registration error: {ErrorCode} - {ErrorDescription}",
                        error.Code, error.Description);
                }

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }

            _logger.LogInformation("Registration successful for {Username}, {Email}",
                registerDto.UserName, registerDto.Email);

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

            // Get the frontend URL from configuration
            var frontendUrl = _configuration["FrontendUrl"] ?? "https://localhost:3000";

            if (result)
            {
                // Redirect to the login page with a success parameter
                return Redirect($"{frontendUrl}/login?verified=true");
            }
            else
            {
                // Redirect to the login page with an error parameter
                return Redirect($"{frontendUrl}/login?verified=false");
            }
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

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordDto emailDto)
        {
            if (string.IsNullOrEmpty(emailDto.Email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            await _identityService.ResendVerificationEmailAsync(emailDto.Email);

            // Always return success to prevent email enumeration attacks
            return Ok(new { message = "If your email exists in our system, you will receive a verification email." });
        }

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