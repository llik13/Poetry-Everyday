using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface ILikeService
    {
        Task<bool> LikePoemAsync(Guid poemId, Guid userId, string userName);
        Task<bool> UnlikePoemAsync(Guid poemId, Guid userId);
        Task<bool> IsPoemLikedByUserAsync(Guid poemId, Guid userId);
    }

}
