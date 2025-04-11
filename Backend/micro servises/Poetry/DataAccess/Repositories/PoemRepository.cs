using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.Repositories
{
    public class PoemRepository : GenericRepository<Poem>, IPoemRepository
    {
        public PoemRepository(PoetryDbContext context) : base(context) { }

        public async Task<IEnumerable<Poem>> GetPoemsByAuthorIdAsync(Guid authorId)
        {
            return await _dbSet
                .Where(p => p.AuthorId == authorId && p.IsPublished)
                .Include(p => p.Statistics)
                .Include(p => p.Tags)
                .Include(p => p.Categories)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Poem>> GetPoemsByTagsAsync(List<string> tagNames)
        {
            return await _dbSet
                .Where(p => p.IsPublished && p.Tags.Any(t => tagNames.Contains(t.Name)))
                .Include(p => p.Statistics)
                .Include(p => p.Tags)
                .Include(p => p.Categories)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Poem>> GetPoemsByCategoryAsync(string categoryName)
        {
            return await _dbSet
                .Where(p => p.IsPublished && p.Categories.Any(c => c.Name == categoryName))
                .Include(p => p.Statistics)
                .Include(p => p.Tags)
                .Include(p => p.Categories)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Poem> GetPoemWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Statistics)
                .Include(p => p.Tags)
                .Include(p => p.Categories)
                .Include(p => p.Comments.OrderByDescending(c => c.CreatedAt).Take(20))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(IEnumerable<Poem> Poems, int TotalCount)> SearchPoemsAsync(
            string searchTerm,
            int pageNumber,
            int pageSize,
            string sortBy,
            bool sortDescending,
            bool isPublished)
        {
            var query = _dbSet
                 .Where(p => p.IsPublished == isPublished);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    p.Excerpt.Contains(searchTerm) ||
                    p.AuthorName.Contains(searchTerm) ||
                    p.Content.Contains(searchTerm) ||
                    p.Tags.Any(t => t.Name.Contains(searchTerm)));
            }

            // Include related entities
            query = query
                .Include(p => p.Statistics)
                .Include(p => p.Tags)
                .Include(p => p.Categories);

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, sortBy, sortDescending);

            // Apply pagination
            var poems = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (poems, totalCount);
        }

        private IQueryable<Poem> ApplySorting(IQueryable<Poem> query, string sortBy, bool sortDescending)
        {
            return sortBy.ToLower() switch
            {
                "title" => sortDescending
                    ? query.OrderByDescending(p => p.Title)
                    : query.OrderBy(p => p.Title),

                "createdat" => sortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),

                "updatedat" => sortDescending
                    ? query.OrderByDescending(p => p.UpdatedAt)
                    : query.OrderBy(p => p.UpdatedAt),

                "viewcount" => sortDescending
                    ? query.OrderByDescending(p => p.Statistics.ViewCount)
                    : query.OrderBy(p => p.Statistics.ViewCount),

                "likecount" => sortDescending
                    ? query.OrderByDescending(p => p.Statistics.LikeCount)
                    : query.OrderBy(p => p.Statistics.LikeCount),

                "commentcount" => sortDescending
                    ? query.OrderByDescending(p => p.Statistics.CommentCount)
                    : query.OrderBy(p => p.Statistics.CommentCount),

                _ => sortDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };
        }

        public async Task<int> GetTotalPoemsCountAsync(string searchTerm)
        {
            var query = _dbSet
                .Where(p => p.IsPublished);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(searchTerm) ||
                    p.Excerpt.Contains(searchTerm) ||
                    p.Content.Contains(searchTerm) ||
                    p.Tags.Any(t => t.Name.Contains(searchTerm)));
            }

            return await query.CountAsync();
        }
    }
}
