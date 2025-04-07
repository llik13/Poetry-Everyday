using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.User.Entities
{
    public class UserActivity
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ActivityType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? RelatedId { get; set; }
        public string? RelatedTitle { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
