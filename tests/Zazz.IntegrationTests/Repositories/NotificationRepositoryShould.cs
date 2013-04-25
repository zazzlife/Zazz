﻿using System;
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
        private Comment _comment1;
        private Comment _comment2;
        private Notification _comment1Notification;
        private Notification _comment2Notification;

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

            _comment1 = Mother.GetComment(_user.Id);
            _comment2 = Mother.GetComment(_user.Id);

            _context.Comments.Add(_comment1);
            _context.Comments.Add(_comment2);

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

            _comment1Notification = Mother.GetNotification(_user.Id);
            _comment1Notification.CommentId = _comment1.Id;
            _comment2Notification = Mother.GetNotification(_user.Id);
            _comment2Notification.CommentId = _comment2.Id;

            _context.Notifications.Add(_photo1Notification);
            _context.Notifications.Add(_photo2Notification);
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

            var photo1Notification = Mother.GetNotification(user2.Id);
            photo1Notification.PhotoId = _photo1.Id;

            var photo2Notification = Mother.GetNotification(user2.Id);
            photo2Notification.PhotoId = _photo2.Id;

            var post1Notification = Mother.GetNotification(user2.Id);
            post1Notification.PostId = _post1.Id;

            var post2Notification = Mother.GetNotification(user2.Id);
            post2Notification.PostId = _post2.Id;

            var event1Notification = Mother.GetNotification(user2.Id);
            event1Notification.EventId = _event1.Id;

            var event2Notification = Mother.GetNotification(user2.Id);
            event2Notification.EventId = _event2.Id;

            _context.Notifications.Add(photo1Notification);
            _context.Notifications.Add(photo2Notification);
            _context.Notifications.Add(post1Notification);
            _context.Notifications.Add(post2Notification);
            _context.Notifications.Add(event1Notification);
            _context.Notifications.Add(event2Notification);
            _context.SaveChanges();

            //Act
            var notifications = _repo.GetUserNotifications(_user.Id);

            //Assert
            Assert.IsTrue(_context.Notifications.Count() > 6);
            Assert.AreEqual(8, notifications.Count());
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

            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));

            //Act
            _repo.RemoveRecordsByPhotoId(_photo1.Id);
            _context.SaveChanges();

            //Assert
            Assert.AreEqual(0, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));
        }

        [Test]
        public void RemoveCorrectRecordsOn_RemoveByPostId()
        {
            //Arrange
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));

            //Act
            _repo.RemoveRecordsByPostId(_post1.Id);
            _context.SaveChanges();

            //Assert
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(0, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));
        }

        [Test]
        public void RemoveCorrectRecordsOn_RemoveByEventId()
        {
            //Arrange
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));

            //Act
            _repo.RemoveRecordsByEventId(_event1.Id);
            _context.SaveChanges();

            //Assert
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(0, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));
        }

        [Test]
        public void RemoveCorrectRecordsOn_RemoveByCommentId()
        {
            //Arrange
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));

            //Act
            _repo.RemoveRecordsByCommentId(_comment1.Id);
            _context.SaveChanges();

            //Assert
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PhotoId == _photo2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.PostId == _post2.Id));

            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.EventId == _event2.Id));

            Assert.AreEqual(0, _context.Notifications.Count(n => n.CommentId == _comment1.Id));
            Assert.AreEqual(1, _context.Notifications.Count(n => n.CommentId == _comment2.Id));
        }

        [Test]
        public void MarkCorrectRecordsAsRead_MarkUserNotificationsAsRead()
        {
            //Arrange
            var user2 = Mother.GetUser();
            _context.Users.Add(user2);
            _context.SaveChanges();

            var photo1Notification = Mother.GetNotification(user2.Id);
            photo1Notification.PhotoId = _photo1.Id;

            var photo2Notification = Mother.GetNotification(user2.Id);
            photo2Notification.PhotoId = _photo2.Id;

            var post1Notification = Mother.GetNotification(user2.Id);
            post1Notification.PostId = _post1.Id;

            var post2Notification = Mother.GetNotification(user2.Id);
            post2Notification.PostId = _post2.Id;

            var event1Notification = Mother.GetNotification(user2.Id);
            event1Notification.EventId = _event1.Id;

            var event2Notification = Mother.GetNotification(user2.Id);
            event2Notification.EventId = _event2.Id;

            _context.Notifications.Add(photo1Notification);
            _context.Notifications.Add(photo2Notification);
            _context.Notifications.Add(post1Notification);
            _context.Notifications.Add(post2Notification);
            _context.Notifications.Add(event1Notification);
            _context.Notifications.Add(event2Notification);
            _context.SaveChanges();

            Assert.AreEqual(8, _context.Notifications.Count(n => n.UserId == _user.Id && !n.IsRead));
            Assert.AreEqual(6, _context.Notifications.Count(n => n.UserId == user2.Id && !n.IsRead));

            //Act
            _repo.MarkUserNotificationsAsRead(_user.Id);
            _context.SaveChanges();

            //Assert
            Assert.AreEqual(0, _context.Notifications.Count(n => n.UserId == _user.Id && !n.IsRead));
            Assert.AreEqual(8, _context.Notifications.Count(n => n.UserId == _user.Id && n.IsRead));
            Assert.AreEqual(6, _context.Notifications.Count(n => n.UserId == user2.Id && !n.IsRead));
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
            Assert.AreEqual(7, _context.Notifications.Count(n => n.UserId == _user.Id && !n.IsRead));
            Assert.AreEqual(7, result);
        }


    }
}