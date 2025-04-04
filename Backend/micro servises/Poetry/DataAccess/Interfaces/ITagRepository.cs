using DataAccess.Entities;
using DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        Task<Tag> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count);
    }

}
