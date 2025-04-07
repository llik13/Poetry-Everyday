using BLL.User.DTOs;
using DAL.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal ValidateAccessToken(string token);
        Task<bool> SaveRefreshTokenAsync(string userId, string token);
        Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
        Task<bool> RevokeAllUserTokensAsync(string userId);
    }

}
