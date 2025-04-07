using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(string fileName, Stream fileStream);
        Task<bool> DeleteFileAsync(string fileUrl);
    }

}
