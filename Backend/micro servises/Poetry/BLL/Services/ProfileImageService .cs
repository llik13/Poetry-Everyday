using BLL.User.Interfaces;
using DAL.User.Entities;
using DAL.User.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Services
{
    public class ProfileImageService : IProfileImageService
    {
        private readonly IProfileImageRepository _profileImageRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileImageService(
            IProfileImageRepository profileImageRepository,
            UserManager<ApplicationUser> userManager)
        {
            _profileImageRepository = profileImageRepository;
            _userManager = userManager;
        }

        public async Task<string> SaveProfileImageAsync(string userId, Stream imageStream)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            // Удаляем старое изображение, если оно существует
            await DeleteProfileImageAsync(userId);

            // Читаем поток в массив байтов
            using (var memoryStream = new MemoryStream())
            {
                await imageStream.CopyToAsync(memoryStream);

                // Определяем тип контента
                string contentType = "image/jpeg"; // По умолчанию JPEG, но лучше определять на основе содержимого

                // Создаем новую сущность для изображения профиля
                var profileImage = new ProfileImage
                {
                    UserId = userId,
                    FileName = $"profile_{Guid.NewGuid()}.jpg",
                    ContentType = contentType,
                    ImageData = memoryStream.ToArray(),
                    UploadedAt = DateTime.UtcNow
                };

                // Сохраняем в базу данных
                await _profileImageRepository.SaveImageAsync(profileImage);
            }

            // Обновляем URL изображения в профиле пользователя
            string imageUrl = $"/api/profile/image/{userId}";
            user.ProfileImageUrl = imageUrl;
            await _userManager.UpdateAsync(user);

            return imageUrl;
        }

        public async Task<bool> DeleteProfileImageAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Удаляем запись об изображении из профиля пользователя
                user.ProfileImageUrl = null;
                await _userManager.UpdateAsync(user);
            }

            // Удаляем изображение из базы данных
            return await _profileImageRepository.DeleteImageAsync(userId);
        }

        public async Task<(byte[] ImageData, string ContentType)> GetProfileImageAsync(string userId)
        {
            var image = await _profileImageRepository.GetImageByUserIdAsync(userId);
            if (image != null)
            {
                return (image.ImageData, image.ContentType);
            }

            return (null, null);
        }
    }
}
