using DAL.User.Context;
using DAL.User.Entities;
using DAL.User.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.User.Repositories
{
    public class UserActivityRepository : IUserActivityRepository
    {
        private readonly IdentityDbContext _context;

        public UserActivityRepository(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddActivityAsync(UserActivity activity)
        {
            _context.UserActivities.Add(activity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 10)
        {
            return await _context.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUserActivityCountAsync(string userId)
        {
            return await _context.UserActivities
                .CountAsync(a => a.UserId == userId);
        }
    }

}
