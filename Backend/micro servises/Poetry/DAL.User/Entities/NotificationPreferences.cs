using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.User.Entities
{
    public class NotificationPreferences
    {
        public bool EmailNotifications { get; set; } = true;
        public bool ForumReplies { get; set; } = true;
        public bool PoemComments { get; set; } = true;
        public bool PoemLikes { get; set; } = true;
        public bool Newsletter { get; set; } = true;
    }

}
