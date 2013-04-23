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
            _uow.NotificationRepository.RemoveRecordsByPhotoId(photoId);
            _uow.SaveChanges();
        }

        public void RemovePostNotifications(int postId)
        {
            _uow.NotificationRepository.RemoveRecordsByPostId(postId);
            _uow.SaveChanges();
        }

        public void RemoveEventNotifications(int eventId)
        {
            _uow.NotificationRepository.RemoveRecordsByEventId(eventId);
            _uow.SaveChanges();
        }

        public void MarkUserNotificationsAsRead(int userId)
        {
            _uow.NotificationRepository.MarkUserNotificationsAsRead(userId);
            _uow.SaveChanges();
        }

        public int GetUnreadNotificationsCount(int userId)
        {
            return _uow.NotificationRepository.GetUnreadNotificationsCount(userId);
        }
    }
}