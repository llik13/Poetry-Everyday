using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.User.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string email, string token, string userId);
        Task<bool> SendPasswordResetEmailAsync(string email, string token);
        Task<bool> SendWelcomeEmailAsync(string email);
        Task<bool> SendNotificationEmailAsync(string email, string subject, string message);
    }

}
