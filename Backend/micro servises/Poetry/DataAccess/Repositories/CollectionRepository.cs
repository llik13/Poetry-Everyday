using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class CollectionRepository : GenericRepository<Collection>, ICollectionRepository
    {
        public CollectionRepository(PoetryDbContext context) : base(context) { }

        public async Task<IEnumerable<Collection>> GetUserCollectionsAsync(Guid userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<Collection> GetCollectionWithPoemsAsync(Guid collectionId)
        {
            return await _dbSet
                .Include(c => c.SavedPoems)
                .FirstOrDefaultAsync(c => c.Id == collectionId);
        }
    }
}
