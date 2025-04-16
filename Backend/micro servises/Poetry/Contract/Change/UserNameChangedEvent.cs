using Contract.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Change
{
    public class UserNameChangedEvent : IUserNameChangedEvent
    {
        public string UserId { get; set; }
        public string OldUserName { get; set; }
        public string NewUserName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
