using DAL.User.Entities;

namespace DAL.User.Interfaces
{
    public interface IProfileImageRepository
    {
        Task<bool> SaveImageAsync(ProfileImage image);
        Task<bool> DeleteImageAsync(string userId);
        Task<ProfileImage> GetImageByUserIdAsync(string userId);
    }
}