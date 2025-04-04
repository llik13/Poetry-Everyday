using DataAccess.Interfaces;
using DataAccess.Entities;
using DataAccess.Context;

namespace DataAccess.Repositories
{
    public class LikeRepository : GenericRepository<Like>, ILikeRepository
    {
        public LikeRepository(PoetryDbContext context) : base(context)
        {
        }
    }
}
