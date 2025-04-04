using BusinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly);
        Task<bool> MarkNotificationAsReadAsync(Guid id, Guid userId);
        Task<bool> MarkAllNotificationsAsReadAsync(Guid userId);
    }

}
