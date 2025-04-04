using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public class SavedPoemRepository : GenericRepository<SavedPoem>, ISavedPoemRepository
    {
        public SavedPoemRepository(PoetryDbContext context) : base(context) { }
    }
}
