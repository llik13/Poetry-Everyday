using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Interfaces
{
    public interface IProfileImageService
    {
        Task<string> SaveProfileImageAsync(string userId, Stream imageStream);
        Task<bool> DeleteProfileImageAsync(string userId);
        Task<(byte[] ImageData, string ContentType)> GetProfileImageAsync(string userId);
    }
}
