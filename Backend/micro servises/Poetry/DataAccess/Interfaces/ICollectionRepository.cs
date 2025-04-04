using DataAccess.Entities;
using DataAccess.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface ICollectionRepository : IGenericRepository<Collection>
    {
        Task<IEnumerable<Collection>> GetUserCollectionsAsync(Guid userId);
        Task<Collection> GetCollectionWithPoemsAsync(Guid collectionId);
    }

}
