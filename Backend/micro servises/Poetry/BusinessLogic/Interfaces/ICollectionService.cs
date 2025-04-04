using BusinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface ICollectionService
    {
        Task<CollectionDto> CreateCollectionAsync(CreateCollectionDto collectionDto);
        Task<IEnumerable<CollectionDto>> GetUserCollectionsAsync(Guid userId);
        Task<CollectionDetailsDto> GetCollectionAsync(Guid id, Guid userId);
        Task<bool> AddPoemToCollectionAsync(Guid collectionId, Guid poemId, Guid userId);
        Task<bool> RemovePoemFromCollectionAsync(Guid collectionId, Guid poemId, Guid userId);
        Task<bool> DeleteCollectionAsync(Guid id, Guid userId);
    }

}
