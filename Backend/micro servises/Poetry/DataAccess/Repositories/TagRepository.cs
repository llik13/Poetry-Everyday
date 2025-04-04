using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(PoetryDbContext context) : base(context) { }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count)
        {
            return await _dbSet
                .OrderByDescending(t => t.Poems.Count)
                .Take(count)
                .ToListAsync();
        }
    }

}
