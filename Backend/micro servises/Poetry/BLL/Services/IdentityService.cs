using BLL.User.DTOs;
using BLL.User.Interfaces;
using DAL.User.Entities;
using DAL.User.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BLL.User.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IUserActivityRepository _activityRepository;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ITokenService tokenService,
            IEmailService emailService,
            IUserActivityRepository activityRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _activityRepository = activityRepository;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterUserDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var roleExist = await _roleManager.RoleExistsAsync("User");
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }
                await _userManager.AddToRoleAsync(user, "User");

                // Generate and send verification token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendVerificationEmailAsync(user.Email, token, user.Id);

                // Log registration activity
                await _activityRepository.AddActivityAsync(new UserActivity
                {
                    UserId = user.Id,
                    Type = ActivityType.Register,
                    Description = "User registered an account"
                });
            }

            return result;
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
                return null;

            if (!user.IsActive)
                return null;

            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, false);

            if (!result.Succeeded)
                return null;

            if (!user.EmailConfirmed)
                return null;

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            // Update last login time
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Log login activity
            await _activityRepository.AddActivityAsync(new UserActivity
            {
                UserId = user.Id,
                Type = ActivityType.Login,
                Description = "User logged in"
            });

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                UserName = user.UserName,
                ExpiresIn = 3600 // 1 hour
            };
        }

        public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
        {
            var validation = await _tokenService.ValidateRefreshTokenAsync(refreshToken);

            if (!validation.IsValid)
                return null;

            var user = await _userManager.FindByIdAsync(validation.UserId);

            if (user == null || !user.IsActive)
                return null;

            var accessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _tokenService.RevokeRefreshTokenAsync(refreshToken);
            await _tokenService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

            return new AuthResultDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                UserId = user.Id,
                UserName = user.UserName,
                ExpiresIn = 3600 // 1 hour
            };
        }

        public async Task<bool> VerifyEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                await _emailService.SendWelcomeEmailAsync(user.Email);

            return result.Succeeded;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return false; // Don't reveal user existence

            if (!user.EmailConfirmed)
                return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);

            return true;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Invalid token." });

            return await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return false;

            return await _tokenService.RevokeRefreshTokenAsync(refreshToken);
        }
    }

}
