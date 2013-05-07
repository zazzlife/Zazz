using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Data;
using Zazz.Data.Repositories;

namespace Zazz.IntegrationTests.Repositories
{
    [TestFixture]
    public class NotificationRepositoryShould
    {
        private ZazzDbContext _context;
        private NotificationRepository _repo;
        private User _user;
        private Photo _photo1;
        private Photo _photo2;
        private Post _post1;
        private Post _post2;
        private ZazzEvent _event1;
        private ZazzEvent _event2;
        private Notification _post1Notification;
        private Notification _post2Notification;
        private Notification _event1Notification;
        private Notification _event2Notification;
        private Comment _comment1;
        private Comment _comment2;
        private Notification _comment1Notification;
        private Notification _comment2Notification;
        private User _userB;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new NotificationRepository(_context);

            _user = Mother.GetUser();
            _userB = Mother.GetUser();
            _context.Users.Add(_user);
            _context.Users.Add(_userB);
            _context.SaveChanges();

            _photo1 = Mother.GetPhoto(_user.Id);
            _photo2 = Mother.GetPhoto(_user.Id);

            _context.Photos.Add(_photo1);
            _context.Photos.Add(_photo2);

            _post1 = Mother.GetPost(_user.Id);
            _post2 = Mother.GetPost(_user.Id);

            _context.Posts.Add(_post1);
            _context.Posts.Add(_post2);

            _event1 = Mother.GetEvent(_user.Id);
            _event2 = Mother.GetEvent(_user.Id);

            _context.Events.Add(_event1);
            _context.Events.Add(_event2);

            _comment1 = Mother.GetComment(_user.Id);
            _comment2 = Mother.GetComment(_user.Id);

            _context.Comments.Add(_comment1);
            _context.Comments.Add(_comment2);

            _context.SaveChanges();

            _post1Notification = Mother.GetNotification(_user.Id, _userB.Id);
            _post1Notification.PostNotification = new PostNotification { PostId = _post1.Id };
            _post1Notification.NotificationType = NotificationType.WallPost;

            _post2Notification = Mother.GetNotification(_user.Id, _userB.Id);
            _post2Notification.PostNotification = new PostNotification { PostId = _post2.Id };
            _post2Notification.NotificationType = NotificationType.WallPost;

            _event1Notification = Mother.GetNotification(_user.Id, _userB.Id);
            _event1Notification.EventNotification = new EventNotification { EventId = _event1.Id };
            _event1Notification.NotificationType = NotificationType.NewEvent;

            _event2Notification = Mother.GetNotification(_user.Id, _userB.Id);
            _event2Notification.EventNotification = new EventNotification { EventId = _event2.Id };
            _event2Notification.NotificationType = NotificationType.NewEvent;

            _comment1Notification = Mother.GetNotification(_user.Id, _userB.Id);
            _comment1Notification.CommentNotification = new CommentNotification { CommentId = _comment1.Id };
            _comment1Notification.NotificationType = NotificationType.CommentOnEvent;

            _comment2Notification = Mother.GetNotification(_user.Id, _userB.Id);
            _comment2Notification.CommentNotification = new CommentNotification { CommentId = _comment2.Id };
            _comment2Notification.NotificationType = NotificationType.CommentOnPost;

            _context.Notifications.Add(_post1Notification);
            _context.Notifications.Add(_post2Notification);
            _context.Notifications.Add(_event1Notification);
            _context.Notifications.Add(_event2Notification);
            _context.Notifications.Add(_comment1Notification);
            _context.Notifications.Add(_comment2Notification);
            _context.SaveChanges();
        }

        [Test]
        public void ReturnCorrectUserNotifications_OnGetUserNotifications()
        {
            //Arrange
            var user2 = Mother.GetUser();
            _context.Users.Add(user2);
            _context.SaveChanges();

            var post1Notification = Mother.GetNotification(user2.Id, _user.Id);
            post1Notification.PostNotification = new PostNotification { PostId = _post1.Id };

            var post2Notification = Mother.GetNotification(user2.Id, _user.Id);
            post2Notification.PostNotification = new PostNotification { PostId = _post2.Id };

            var event1Notification = Mother.GetNotification(user2.Id, _user.Id);
            event1Notification.EventNotification = new EventNotification {EventId = _event1.Id};

            var event2Notification = Mother.GetNotification(user2.Id, _user.Id);
            event2Notification.EventNotification = new EventNotification {EventId = _event2.Id};

            _context.Notifications.Add(post1Notification);
            _context.Notifications.Add(post2Notification);
            _context.Notifications.Add(event1Notification);
            _context.Notifications.Add(event2Notification);
            _context.SaveChanges();

            //Act
            var notifications = _repo.GetUserNotifications(_user.Id, null);

            //Assert
            Assert.IsTrue(_context.Notifications.Count() > 6);
            Assert.AreEqual(6, notifications.Count());
        }

        [Test]
        public void RemoveCorrectRecords_OnRemoveFollowAcceptedNotifications()
        {
            //Arrange
            var notification = Mother.GetNotification(_user.Id, _userB.Id);
            notification.NotificationType = NotificationType.FollowRequestAccepted;

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            Assert.IsTrue(_context.Notifications
                                  .Where(n => n.NotificationType == NotificationType.FollowRequestAccepted)
                                  .Where(n => n.UserId == _user.Id)
                                  .Where(n => n.UserBId == _userB.Id)
                                  .Any());

            //Act
            _repo.RemoveFollowAcceptedNotification(notification.UserId, notification.UserBId);
            _context.SaveChanges();

            //Assert
            Assert.IsFalse(_context.Notifications
                                  .Where(n => n.NotificationType == NotificationType.FollowRequestAccepted)
                                  .Where(n => n.UserId == _user.Id)
                                  .Where(n => n.UserBId == _userB.Id)
                                  .Any());
        }

        [Test]
        public void MarkCorrectRecordsAsRead_MarkUserNotificationsAsRead()
        {
            //Arrange
            var user2 = Mother.GetUser();
            _context.Users.Add(user2);
            _context.SaveChanges();

            var post1Notification = Mother.GetNotification(user2.Id, _user.Id);
            post1Notification.PostNotification = new PostNotification { PostId = _post1.Id };

            var post2Notification = Mother.GetNotification(user2.Id, _user.Id);
            post2Notification.PostNotification = new PostNotification { PostId = _post2.Id };

            var event1Notification = Mother.GetNotification(user2.Id, _user.Id);
            event1Notification.EventNotification = new EventNotification {EventId = _event1.Id};

            var event2Notification = Mother.GetNotification(user2.Id, _user.Id);
            event2Notification.EventNotification = new EventNotification {EventId = _event2.Id};

            _context.Notifications.Add(post1Notification);
            _context.Notifications.Add(post2Notification);
            _context.Notifications.Add(event1Notification);
            _context.Notifications.Add(event2Notification);
            _context.SaveChanges();

            Assert.AreEqual(6, _context.Notifications.Count(n => n.UserId == _user.Id && !n.IsRead));
            Assert.AreEqual(4, _context.Notifications.Count(n => n.UserId == user2.Id && !n.IsRead));

            //Act
            _repo.MarkUserNotificationsAsRead(_user.Id);
            _context.SaveChanges();

            //Assert
            Assert.AreEqual(0, _context.Notifications.Count(n => n.UserId == _user.Id && !n.IsRead));
            Assert.AreEqual(6, _context.Notifications.Count(n => n.UserId == _user.Id && n.IsRead));
            Assert.AreEqual(4, _context.Notifications.Count(n => n.UserId == user2.Id && !n.IsRead));
        }

        [Test]
        public void RetrunCorrectNewNotificationsCount()
        {
            //Arrange
            var randomNotification = _context.Notifications.First();
            randomNotification.IsRead = true;

            _context.SaveChanges();

            //Act
            var result = _repo.GetUnreadNotificationsCount(_user.Id);

            //Assert
            Assert.AreEqual(5, _context.Notifications.Count(n => n.UserId == _user.Id && !n.IsRead));
            Assert.AreEqual(5, result);
        }

        [Test]
        public void ReturnNotificationsWithSmallerIdThanTheProvidedLastNotificationId_OnGetUserNotifications()
        {
            //Arrange
            var notificationWithLargestId = _context.Notifications
                                                    .OrderByDescending(n => n.Id)
                                                    .First();

            //Act
            var result = _repo.GetUserNotifications(_user.Id, notificationWithLargestId.Id);

            //Assert
            CollectionAssert.DoesNotContain(result, notificationWithLargestId);
        }
    }
}