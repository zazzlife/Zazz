using System;
using System.Linq;
using NUnit.Framework;
using Zazz.Core.Models.Data;
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
        private Notification _photo1Notification;
        private Notification _photo2Notification;
        private Notification _post1Notification;
        private Notification _post2Notification;
        private Notification _event1Notification;
        private Notification _event2Notification;

        [SetUp]
        public void Init()
        {
            _context = new ZazzDbContext(true);
            _repo = new NotificationRepository(_context);

            _user = Mother.GetUser();
            _context.Users.Add(_user);
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

            _context.SaveChanges();

            _photo1Notification = Mother.GetNotification(_user.Id);
            _photo1Notification.PhotoId = _photo1.Id;

            _photo2Notification = Mother.GetNotification(_user.Id);
            _photo2Notification.PhotoId = _photo2.Id;

            _post1Notification = Mother.GetNotification(_user.Id);
            _post1Notification.PostId = _post1.Id;

            _post2Notification = Mother.GetNotification(_user.Id);
            _post2Notification.PostId = _post2.Id;

            _event1Notification = Mother.GetNotification(_user.Id);
            _event1Notification.EventId = _event1.Id;

            _event2Notification = Mother.GetNotification(_user.Id);
            _event2Notification.EventId = _event2.Id;

            _context.Notifications.Add(_photo1Notification);
            _context.Notifications.Add(_photo2Notification);
            _context.Notifications.Add(_post1Notification);
            _context.Notifications.Add(_post2Notification);
            _context.Notifications.Add(_event1Notification);
            _context.Notifications.Add(_event2Notification);
            _context.SaveChanges();
        }

        [Test]
        public void RemoveCorrectRecordsOn_RemoveByPhotoId()
        {
            //Arrange
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            //Act
            _repo.RemoveRecordsByPhotoId(_post1.Id);
            _context.SaveChanges();

            //Assert
            Assert.AreEqual(0, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));
        }


    }
}