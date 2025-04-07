using BLL.User.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly string _storageBasePath;

        public FileStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _storageBasePath = _configuration["Storage:BasePath"] ?? "wwwroot/uploads";
        }

        public async Task<string> SaveFileAsync(string fileName, Stream fileStream)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(Path.Combine(_storageBasePath, fileName));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Save the file
            var filePath = Path.Combine(_storageBasePath, fileName);
            using (var fileWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileWriter);
            }

            // Return a URL that can be used to access the file
            return $"/uploads/{fileName.Replace("\\", "/")}";
        }

        public Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return Task.FromResult(false);

                // Extract the file path from the URL
                string relativePath = fileUrl.Replace("/uploads/", "");
                string filePath = Path.Combine(_storageBasePath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }

}
