using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class NotificationServiceShould
    {
        private Mock<IUoW> _uow;
        private NotificationService _sut;
        private Notification _notification;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new NotificationService(_uow.Object);

            _uow.Setup(x => x.SaveChanges());

            _notification = new Notification();
        }

        [Test]
        public void ReturnResultFromRepository_OnGetUserNotifications()
        {
            //Arrange
            var userId = 13456;
            var notifications = new List<Notification>();
            _uow.Setup(x => x.NotificationRepository.GetUserNotifications(userId))
                .Returns(notifications.AsQueryable());

            //Act
            var result = _sut.GetUserNotifications(userId);

            //Assert
            Assert.IsNotNull(result);
            _uow.Verify(x => x.NotificationRepository.GetUserNotifications(userId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CreateNotificationAndSaveIfSaveIsNotProvided_OnCreateNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(_notification));

            //Act
            _sut.CreateNotification(_notification);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(_notification), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNotificationAndNotSaveIfSaveIsFalse_OnCreateNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(_notification));

            //Act
            _sut.CreateNotification(_notification, false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(_notification), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CallRemovePhotoRecordsOnRepository_OnRemovePhotoNotifications()
        {
            //Arrange
            var photoId = 12;
            _uow.Setup(x => x.NotificationRepository.RemoveRecordsByPhotoId(photoId));

            //Act
            _sut.RemovePhotoNotifications(photoId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.RemoveRecordsByPhotoId(photoId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CallRemovePostRecordsOnRepository_OnRemovePostNotifications()
        {
            //Arrange
            var postId = 12;
            _uow.Setup(x => x.NotificationRepository.RemoveRecordsByPostId(postId));

            //Act
            _sut.RemovePostNotifications(postId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.RemoveRecordsByPostId(postId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CallRemoveEventRecordsOnRepository_OnRemoveEventNotifications()
        {
            //Arrange
            var eventId = 12;
            _uow.Setup(x => x.NotificationRepository.RemoveRecordsByEventId(eventId));

            //Act
            _sut.RemoveEventNotifications(eventId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.RemoveRecordsByEventId(eventId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CallMarkNotificationsAsReadOnRepository_OnNotificationsAsRead()
        {
            //Arrange
            var userId = 12;
            _uow.Setup(x => x.NotificationRepository.MarkUserNotificationsAsRead(userId));

            //Act
            _sut.MarkUserNotificationsAsRead(userId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.MarkUserNotificationsAsRead(userId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ReturnResutFromRepository_OnGetUnreadNotificationsCount()
        {
            //Arrange
            var userId = 12;
            var count = 3;
            _uow.Setup(x => x.NotificationRepository.GetUnreadNotificationsCount(userId))
                .Returns(count);

            //Act
            var result = _sut.GetUnreadNotificationsCount(userId);

            //Assert
            Assert.AreEqual(count, result);
            _uow.Verify(x => x.NotificationRepository.GetUnreadNotificationsCount(userId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }
    }
}