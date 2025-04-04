using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(PoetryDbContext context) : base(context) { }

        public async Task<IEnumerable<Comment>> GetCommentsByPoemIdAsync(Guid poemId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Where(c => c.PoemId == poemId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
