﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.DTOs
{
    public class NotificationSettingsDto
    {
        public bool EmailNotifications { get; set; } = true;
        public bool ForumReplies { get; set; } = true;
        public bool PoemComments { get; set; } = true;
        public bool PoemLikes { get; set; } = true;
        public bool Newsletter { get; set; } = true;
    }

}
