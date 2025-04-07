using DAL.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.User.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<bool> AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken> GetRefreshTokenAsync(string token);
        Task<bool> RevokeRefreshTokenAsync(string token);
        Task<bool> RevokeAllUserTokensAsync(string userId);
    }

}
