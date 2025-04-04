using DataAccess.Entities;
using DataAccess.Interface;

namespace DataAccess.Interfaces
{
    public interface IPoemRepository : IGenericRepository<Poem>
    {
        Task<IEnumerable<Poem>> GetPoemsByAuthorIdAsync(Guid authorId);
        Task<IEnumerable<Poem>> GetPoemsByTagsAsync(List<string> tagNames);
        Task<IEnumerable<Poem>> GetPoemsByCategoryAsync(string categoryName);
        Task<Poem> GetPoemWithDetailsAsync(Guid id);
        Task<(IEnumerable<Poem> Poems, int TotalCount)> SearchPoemsAsync(string searchTerm, int pageNumber, int pageSize, string sortBy, bool sortDescending);
        Task<int> GetTotalPoemsCountAsync(string searchTerm);
    }
}
