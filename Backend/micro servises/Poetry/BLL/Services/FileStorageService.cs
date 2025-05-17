using BLL.User.Interfaces;
using DAL.User.Entities;
using DAL.User.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BLL.User.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly IProfileImageRepository _profileImageRepository;
        private readonly string _storageBasePath;
        private bool _useDatabase;

        public FileStorageService(IConfiguration configuration, IProfileImageRepository profileImageRepository)
        {
            _configuration = configuration;
            _profileImageRepository = profileImageRepository;
            _storageBasePath = _configuration["Storage:BasePath"] ?? "wwwroot/uploads";

            _useDatabase = _configuration.GetValue<bool>("Storage:UseDatabase", true);
        }

        public async Task<string> SaveFileAsync(string fileName, Stream fileStream)
        {
            if (_useDatabase && fileName.StartsWith("profile-images/"))
            {
                return await SaveFileToDatabase(fileName, fileStream);
            }
            else
            {
                return await SaveFileToFileSystem(fileName, fileStream);
            }
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return false;

                if (_useDatabase && fileUrl.Contains("/api/profile/image/"))
                {
                    string userId = GetUserIdFromUrl(fileUrl);
                    if (string.IsNullOrEmpty(userId))
                        return false;

                    return await _profileImageRepository.DeleteImageAsync(userId);
                }
                else
                {
                    string relativePath = fileUrl.Replace("/uploads/", "");
                    string filePath = Path.Combine(_storageBasePath, relativePath);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<(byte[] ImageData, string ContentType)> GetImageAsync(string userId)
        {
            if (_useDatabase)
            {
                var image = await _profileImageRepository.GetImageByUserIdAsync(userId);
                if (image != null)
                {
                    return (image.ImageData, image.ContentType);
                }
            }
            else
            {
                string[] possibleExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                foreach (var ext in possibleExtensions)
                {
                    string filePath = Path.Combine(_storageBasePath, "profile-images", userId, "profile" + ext);
                    if (File.Exists(filePath))
                    {
                        var imageData = await File.ReadAllBytesAsync(filePath);
                        string contentType = GetContentTypeFromFileName(filePath);
                        return (imageData, contentType);
                    }
                }
            }
            return (null, null);
        }

        private async Task<string> SaveFileToDatabase(string fileName, Stream fileStream)
        {
            string userId = GetUserIdFromFileName(fileName);

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("Invalid file name format. Expected format: profile-images/{userId}/{fileName}");
            }

            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);

                var profileImage = new ProfileImage
                {
                    UserId = userId,
                    FileName = Path.GetFileName(fileName),
                    ContentType = GetContentTypeFromFileName(fileName),
                    ImageData = memoryStream.ToArray(),
                    UploadedAt = DateTime.UtcNow
                };

                await _profileImageRepository.SaveImageAsync(profileImage);
            }

            return $"/api/profile/image/{userId}";
        }

        private async Task<string> SaveFileToFileSystem(string fileName, Stream fileStream)
        {
            var directory = Path.GetDirectoryName(Path.Combine(_storageBasePath, fileName));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var filePath = Path.Combine(_storageBasePath, fileName);
            using (var fileWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileWriter);
            }

            return $"/uploads/{fileName.Replace("\\", "/")}";
        }

        private string GetUserIdFromFileName(string fileName)
        {
            var parts = fileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && parts[0] == "profile-images")
            {
                return parts[1];
            }
            return null;
        }

        private string GetUserIdFromUrl(string url)
        {
            var parts = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && parts[0] == "api" && parts[1] == "profile" && parts[2] == "image")
            {
                return parts[3];
            }
            return null;
        }

        private string GetContentTypeFromFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}