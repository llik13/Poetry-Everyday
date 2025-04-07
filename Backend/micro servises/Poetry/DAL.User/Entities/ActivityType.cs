using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.User.Entities
{
    public enum ActivityType
    {
        Login,
        Register,
        Publish,
        UpdateProfile,
        Comment,
        Like,
        SavePoem,
        ForumPost,
        ForumReply
    }

}
