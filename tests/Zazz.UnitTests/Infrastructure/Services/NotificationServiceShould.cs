using System.Collections.Generic;
using System.Linq;
using System.Security;
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
        private int _commentId;
        private int _commenterId;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new NotificationService(_uow.Object);

            _uow.Setup(x => x.SaveChanges());

            _notification = new Notification();
            _commentId = 12345;
            _commenterId = 555;
        }

        [Test]
        public void ReturnResultFromRepository_OnGetUserNotifications()
        {
            //Arrange
            var userId = 13456;
            var notifications = new List<Notification>();
            _uow.Setup(x => x.NotificationRepository.GetUserNotifications(userId, null))
                .Returns(notifications.AsQueryable());

            //Act
            var result = _sut.GetUserNotifications(userId, null);

            //Assert
            Assert.IsNotNull(result);
            _uow.Verify(x => x.NotificationRepository.GetUserNotifications(userId, null), Times.Once());
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
        public void CreateNewNotificationAndSaveIfSaveIsNotDefined_OnCreateFollowApprovedNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateFollowAcceptedNotification(1, 2);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNewNotificationAndNotSaveIfSaveIsFalse_OnCreateFollowApprovedNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateFollowAcceptedNotification(1, 2, false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CreateNewNotificationAndSaveIfSaveIsNotDefined_OnCreatePhotoCommentNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreatePhotoCommentNotification(_commentId, _commenterId, 1, 2);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNewNotificationAndNotSaveIfSaveIsFalse_OnCreatePhotoCommentNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreatePhotoCommentNotification(_commentId, _commenterId, 1, 2, save: false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CreateNewNotificationAndSaveIfSaveIsNotDefined_OnCreatePostCommentNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreatePostCommentNotification(_commentId, _commenterId, 1, 2);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNewNotificationAndNotSaveIfSaveIsFalse_OnCreatePostCommentNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreatePostCommentNotification(_commentId, _commenterId, 1, 2, save: false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CreateNewNotificationAndSaveIfSaveIsNotDefined_OnCreateEventCommentNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateEventCommentNotification(_commentId, _commenterId, 1, 2);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNewNotificationAndNotSaveIfSaveIsFalse_OnCreateEventCommentNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateEventCommentNotification(_commentId, _commenterId, 1, 2, save: false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CreateNewNotificationAndSaveIfSaveIsNotDefined_OnCreateWallPostNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateWallPostNotification(1, 2, 3);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNewNotificationAndNotSaveIfSaveIsFalse_OnCreateWallPostNotification()
        {
            //Arrange
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateWallPostNotification(1, 2, 3, save: false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CreateNotificationsForAllClubAdminFollowersAndSaveOnceWhenItsNotSpecified_OnCreateNewEventNotification()
        {
            //Arrange
            var userId = 1234;
            var eventId = 12;
            var followers = new List<Follow>
                        {
                            new Follow
                            {
                                FromUserId = 1
                            },
                            new Follow
                            {
                                FromUserId = 2
                            },
                            new Follow
                            {
                                FromUserId = 3
                            }
                        };

            _uow.Setup(x => x.FollowRepository.GetUserFollowers(userId))
                .Returns(followers.AsQueryable());
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateNewEventNotification(userId, eventId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()),
                        Times.Exactly(followers.Count));
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNotificationsForAllClubAdminFollowersAndNotSaveWhenItsFalse_OnCreateNewEventNotification()
        {
            //Arrange
            var userId = 1234;
            var eventId = 12;
            var followers = new List<Follow>
                        {
                            new Follow
                            {
                                FromUserId = 1
                            },
                            new Follow
                            {
                                FromUserId = 2
                            },
                            new Follow
                            {
                                FromUserId = 3
                            }
                        };

            _uow.Setup(x => x.FollowRepository.GetUserFollowers(userId))
                .Returns(followers.AsQueryable());
            _uow.Setup(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()));

            //Act
            _sut.CreateNewEventNotification(userId, eventId, false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.InsertGraph(It.IsAny<Notification>()),
                        Times.Exactly(followers.Count));
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void CallRemoveFollowAcceptedNotificationOnRepo_OnRemoveFollowAcceptedNotification()
        {
            //Arrange
            var fromId = 12;
            var toId = 122;

            _uow.Setup(x => x.NotificationRepository.RemoveFollowAcceptedNotification(fromId, toId));

            //Act
            _sut.RemoveFollowAcceptedNotification(fromId, toId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.RemoveFollowAcceptedNotification(fromId, toId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CallRemoveFollowAcceptedNotificationOnRepoAndNotSaveIfSaveIsFalse_OnRemoveFollowAcceptedNotification()
        {
            //Arrange
            var fromId = 12;
            var toId = 122;

            _uow.Setup(x => x.NotificationRepository.RemoveFollowAcceptedNotification(fromId, toId));

            //Act
            _sut.RemoveFollowAcceptedNotification(fromId, toId, false);

            //Assert
            _uow.Verify(x => x.NotificationRepository.RemoveFollowAcceptedNotification(fromId, toId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
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

        [Test]
        public void NotThrowIfNotificationDoesntExists_OnRemove()
        {
            //Arrange
            var id = 123;
            var currentUserId = 1;
            _uow.Setup(x => x.NotificationRepository.GetById(id))
                .Returns(() => null);

            //Act
            _sut.Remove(id, currentUserId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.GetById(id), Times.Once());
        }

        [Test]
        public void ThrowIfCurrentUserIsNotTheOwner_OnRemove()
        {
            //Arrange
            var notification = new Notification
                               {
                                   UserId = 12,
                                   Id = 7
                               };
            _uow.Setup(x => x.NotificationRepository.GetById(notification.Id))
                .Returns(notification);

            //Act
            Assert.Throws<SecurityException>(() => _sut.Remove(notification.Id, 90));

            //Assert
            _uow.Verify(x => x.NotificationRepository.GetById(notification.Id), Times.Once());
            _uow.Verify(x => x.NotificationRepository.Remove(It.IsAny<Notification>()), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void Remove_OnRemove()
        {
            //Arrange
            var notification = new Notification
            {
                UserId = 12,
                Id = 7
            };
            _uow.Setup(x => x.NotificationRepository.GetById(notification.Id))
                .Returns(notification);
            _uow.Setup(x => x.NotificationRepository.Remove(notification));

            //Act
            _sut.Remove(notification.Id, notification.UserId);

            //Assert
            _uow.Verify(x => x.NotificationRepository.GetById(notification.Id), Times.Once());
            _uow.Verify(x => x.NotificationRepository.Remove(notification), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}