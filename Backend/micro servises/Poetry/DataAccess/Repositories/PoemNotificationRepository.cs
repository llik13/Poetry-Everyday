using DataAccess.Context;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public class PoemNotificationRepository : GenericRepository<PoemNotification>, IPoemNotificationRepository
    {
        public PoemNotificationRepository(PoetryDbContext context) : base(context)
        {
        }
    }
}
