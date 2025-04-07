using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Interfaces
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }

}
