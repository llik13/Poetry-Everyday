using DAL.User.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.User.Interfaces
{
    public interface IUserActivityRepository
    {
        Task<bool> AddActivityAsync(UserActivity activity);
        Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 10);
        Task<int> GetUserActivityCountAsync(string userId);
    }

}
