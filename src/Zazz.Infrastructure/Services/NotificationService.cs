using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUoW _uow;

        public NotificationService(IUoW uow)
        {
            _uow = uow;
        }

        public IQueryable<Notification> GetUserNotifications(int userId)
        {
            throw new System.NotImplementedException();
        }

        public void CreateNotification(Notification notification, bool save = true)
        {
            _uow.NotificationRepository.InsertGraph(notification);

            if (save)
                _uow.SaveChanges();
        }

        public void RemovePhotoNotifications(int photoId)
        {
            throw new System.NotImplementedException();
        }

        public void RemovePostNotifications(int postId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveEventNotifications(int eventId)
        {
            throw new System.NotImplementedException();
        }

        public void MarkUserNotificationsAsRead(int userId)
        {
            throw new System.NotImplementedException();
        }

        public int GetUnreadNotificationsCount(int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}