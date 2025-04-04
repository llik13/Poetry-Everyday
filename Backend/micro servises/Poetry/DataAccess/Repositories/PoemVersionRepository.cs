using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Interfaces;


namespace DataAccess.Repositories
{
    public class PoemVersionRepository : GenericRepository<PoemVersion>, IPoemVersionRepository
    {
        public PoemVersionRepository(PoetryDbContext context) : base(context)
        {
        }
    }
}
