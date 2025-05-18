using DAL.User.Context;
using DAL.User.Entities;
using DAL.User.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.User.Repositories
{
    public class ProfileImageRepository : IProfileImageRepository
    {
        private readonly IdentityDbContext _context;

        public ProfileImageRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SaveImageAsync(ProfileImage image)
        {
            try
            {
                // Check if there's an existing image for this user
                var existingImage = await _context.ProfileImages
                    .FirstOrDefaultAsync(p => p.UserId == image.UserId);

                if (existingImage != null)
                {
                    // Update existing image
                    existingImage.FileName = image.FileName;
                    existingImage.ContentType = image.ContentType;
                    existingImage.ImageData = image.ImageData;
                    existingImage.UploadedAt = DateTime.UtcNow;

                    _context.ProfileImages.Update(existingImage);
                }
                else
                {
                    // Add new image
                    _context.ProfileImages.Add(image);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteImageAsync(string userId)
        {
            try
            {
                var image = await _context.ProfileImages
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (image != null)
                {
                    _context.ProfileImages.Remove(image);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ProfileImage> GetImageByUserIdAsync(string userId)
        {
            return await _context.ProfileImages
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}