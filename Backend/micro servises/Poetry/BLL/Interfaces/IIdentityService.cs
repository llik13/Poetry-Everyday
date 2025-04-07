using BLL.User.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Interfaces
{
    public interface IIdentityService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterUserDto registerDto);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> RefreshTokenAsync(string refreshToken);
        Task<bool> VerifyEmailAsync(string userId, string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> LogoutAsync(string refreshToken);
    }

}
