using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Interfaces
{
    public interface IUserNameChangedEvent
    {
        string UserId { get; }
        string OldUserName { get; }
        string NewUserName { get; }
        DateTime Timestamp { get; }
    }
}
