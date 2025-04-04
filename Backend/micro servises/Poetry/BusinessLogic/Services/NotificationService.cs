using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPoemService _poemService;

        public NotificationService(IUnitOfWork unitOfWork, IPoemService poemService)
        {
            _unitOfWork = unitOfWork;
            _poemService = poemService;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool unreadOnly)
        {
            var query = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = query.OrderByDescending(n => n.CreatedAt);
            var result = new List<NotificationDto>();

            foreach (var notification in notifications)
            {
                var poem = await _poemService.GetPoemByIdAsync(notification.PoemId);
                string poemTitle = poem?.Title ?? "Unknown Poem";

                result.Add(new NotificationDto
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    PoemId = notification.PoemId,
                    PoemTitle = poemTitle,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    Type = notification.Type.ToString(),
                    CreatedAt = notification.CreatedAt
                });
            }

            return result;
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid id, Guid userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(
                n => n.Id == id && n.UserId == userId);

            var notification = notifications.FirstOrDefault();
            if (notification == null) return false;

            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> MarkAllNotificationsAsReadAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(
                n => n.UserId == userId && !n.IsRead);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}