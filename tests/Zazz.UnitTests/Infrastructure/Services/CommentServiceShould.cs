using System.Collections.Generic;
using System.Security;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class CommentServiceShould
    {
        private Mock<IUoW> _uow;
        private Mock<INotificationService> _notificationService;
        private CommentService _sut;
        private int _ownerId;
        private Comment _comment;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _notificationService = new Mock<INotificationService>();
            _sut = new CommentService(_uow.Object, _notificationService.Object);

            _uow.Setup(x => x.SaveChanges());

            _ownerId = 444;
            _comment = new Comment
            {
                Id = 12345,
                UserId = 1,
                PhotoComment = new PhotoComment {PhotoId = 2},
                PostId = 3,
                EventId = 4
            };
        }

        [Test]
        public void CreateNotificationAndSaveCommentWhenCommentIsOnPhoto_OnCreateComment()
        {
            //Arrange
            var photo = new Photo
                        {
                            UserId = _ownerId
                        };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PhotoRepository.GetById(_comment.PhotoComment.PhotoId))
                .Returns(photo);

            _notificationService.Setup(x => x.CreatePhotoCommentNotification(
                _comment.Id, _comment.UserId, _comment.PhotoComment.PhotoId, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Photo);

            //Assert
            _uow.Verify(x => x.CommentRepository.InsertGraph(_comment), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetById(_comment.PhotoComment.PhotoId), Times.Once());
            _notificationService.Verify(x => x.CreatePhotoCommentNotification(
                _comment.Id, _comment.UserId, _comment.PhotoComment.PhotoId, _ownerId, false), Times.Once());

            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void NotCreateNotificationIfUserHasCommentedOnHisOwnPhoto_OnCreateComment()
        {
            //Arrange
            var photo = new Photo
                        {
                            UserId = _comment.UserId
                        };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PhotoRepository.GetById(_comment.PhotoComment.PhotoId))
                .Returns(photo);

            _notificationService.Setup(x => x.CreatePhotoCommentNotification(
                _comment.Id, _comment.UserId, _comment.PhotoComment.PhotoId, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Photo);


            //Assert
            _notificationService.Verify(x => x.CreatePhotoCommentNotification(It.IsAny<int>(), It.IsAny<int>(),
                                             It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
        }

        [Test]
        public void SaveCommentWhenCommentIsOnPost_OnCreateComment()
        {
            //Arrange
            var post = new Post
                       {
                           FromUserId = _ownerId
                       };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostId.Value))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId, _comment.PostId.Value, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _uow.Verify(x => x.CommentRepository.InsertGraph(_comment), Times.Once());
            _uow.Verify(x => x.PostRepository.GetById(_comment.PostId.Value), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
        }

        [Test]
        public void NotCreateNotificationIfUserHasCommentedOnHisOwnPost_OnCreateComment()
        {
            //Arrange
            var post = new Post
                       {
                           FromUserId = _comment.UserId
                       };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostId.Value))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId, _comment.PostId.Value, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _notificationService.Verify(x => x.CreatePostCommentNotification(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
        }

        [Test]
        public void NotifyOwnerIfTheCommenterIsSomeoneElse_OnCreateComment()
        {
            //Arrange
            var post = new Post
                       {
                           Id = _comment.PostId.Value,
                           FromUserId = _ownerId
                       };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostId.Value))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId, _comment.PostId.Value, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                post.Id, post.FromUserId, false), Times.Once());
        }

        [Test]
        public void NotifyBothFROM_USERAndTO_USERIfTheCommenterIsSomeoneElse_OnCreateComment()
        {
            //Arrange
            var post = new Post
            {
                Id = _comment.PostId.Value,
                FromUserId = _ownerId,
                ToUserId = 888
            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostId.Value))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                _comment.PostId.Value, It.IsAny<int>(), false));

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                post.Id, post.FromUserId, false), Times.Once());
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                post.Id, post.ToUserId.Value, false), Times.Once());
        }

        [Test]
        public void OnlyNotifyTO_USERIfCommentIsFromFROM_USER_OnCreateComment()
        {
            //Arrange
            var post = new Post
            {
                Id = _comment.PostId.Value,
                FromUserId = _comment.UserId,
                ToUserId = 888
            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostId.Value))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                _comment.PostId.Value, It.IsAny<int>(), false));

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                post.Id, post.FromUserId, false), Times.Never());
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                post.Id, post.ToUserId.Value, false), Times.Once());
        }

        [Test]
        public void OnlyNotifyFROM_USERIfCommentIsFromTO_USER_OnCreateComment()
        {
            //Arrange
            var post = new Post
            {
                Id = _comment.PostId.Value,
                FromUserId = _ownerId,
                ToUserId = _comment.UserId
            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostId.Value))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                _comment.PostId.Value, It.IsAny<int>(), false));

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                post.Id, post.FromUserId, false), Times.Once());
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                post.Id, post.ToUserId.Value, false), Times.Never());
        }

        [Test]
        public void CreateNotificationAndSaveCommentWhenCommentIsOnEvent_OnCreateComment()
        {
            //Arrange
            var zazzEvent = new ZazzEvent
                            {
                                UserId = _ownerId
                            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.EventRepository.GetById(_comment.EventId.Value))
                .Returns(zazzEvent);
            _notificationService.Setup(x => x.CreateEventCommentNotification(_comment.Id, _comment.UserId, _comment.EventId.Value, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Event);

            //Assert
            _uow.Verify(x => x.CommentRepository.InsertGraph(_comment), Times.Once());
            _uow.Verify(x => x.EventRepository.GetById(_comment.EventId.Value), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _notificationService.Verify(x => x.CreateEventCommentNotification(_comment.Id, _comment.UserId, _comment.EventId.Value, _ownerId, false), Times.Once());
        }

        [Test]
        public void NotCreateNotificationIfUserHasCommentedOnHisOwnEvent_OnCreateComment()
        {
            //Arrange
            var zazzEvent = new ZazzEvent
            {
                UserId = _comment.UserId
            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.EventRepository.GetById(_comment.EventId.Value))
                .Returns(zazzEvent);
            _notificationService.Setup(x => x.CreateEventCommentNotification(_comment.Id, _comment.UserId, _comment.EventId.Value, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Event);

            //Assert
            _uow.Verify(x => x.CommentRepository.InsertGraph(_comment), Times.Once());
            _uow.Verify(x => x.EventRepository.GetById(_comment.EventId.Value), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Exactly(2));
            _notificationService.Verify(
                x => x.CreateEventCommentNotification(_comment.Id, _comment.UserId, _comment.EventId.Value, zazzEvent.UserId, false),
                Times.Never());
        }

        [Test]
        public void NotThrowIfCommentDoesntExists_OnEditComment()
        {
            //Arrange
            var commentId = 12;
            _uow.Setup(x => x.CommentRepository.GetById(commentId))
                .Returns(() => null);


            //Act
            _sut.EditComment(commentId, 123, "");

            //Assert
            _uow.Verify(x => x.CommentRepository.GetById(commentId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void ThrowIfTheUserIsNotTheOwner_OnEditComment()
        {
            //Arrange
            var commentId = 12;
            var userId = 123;

            var comment = new Comment
                          {
                              Id = commentId,
                              UserId = userId,
                              Message = "Original Message"
                          };

            _uow.Setup(x => x.CommentRepository.GetById(commentId))
                .Returns(() => comment);


            //Act
            Assert.Throws<SecurityException>(() => _sut.EditComment(commentId, 1, ""));

            //Assert
            _uow.Verify(x => x.CommentRepository.GetById(commentId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void SaveNewCommentIfEverythingIsAlright_OnEditComment()
        {
            //Arrange
            var commentId = 12;
            var userId = 123;
            var newMessage = "New Message";

            var comment = new Comment
                          {
                              Id = commentId,
                              UserId = userId,
                              Message = "Original Message"
                          };

            _uow.Setup(x => x.CommentRepository.GetById(commentId))
                .Returns(() => comment);

            //Act
            _sut.EditComment(commentId, userId, newMessage);

            //Assert
            Assert.AreEqual(newMessage, comment.Message);
            _uow.Verify(x => x.CommentRepository.GetById(commentId), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void NotThrowWhenCommentDoesnTExists_OnRemoveComment()
        {
            //Arrange
            var commentId = 12;
            var userId = 123;

            _uow.Setup(x => x.CommentRepository.GetById(commentId))
                .Returns(() => null);

            //Act
            _sut.RemoveComment(commentId, userId);

            //Assert
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _notificationService.Verify(x => x.RemoveCommentNotifications(commentId), Times.Never());
        }

        [Test]
        public void ThrowIfCommentIsNotFromCurrentUser_OnRemoveComment()
        {
            //Arrange
            var commentId = 12;
            var userId = 123;

            var comment = new Comment
                          {
                              Id = commentId,
                              UserId = userId,
                          };

            _uow.Setup(x => x.CommentRepository.GetById(commentId))
                .Returns(() => comment);

            //Act
            Assert.Throws<SecurityException>(() => _sut.RemoveComment(commentId, 9));

            //Assert
            _uow.Verify(x => x.SaveChanges(), Times.Never());
            _notificationService.Verify(x => x.RemoveCommentNotifications(commentId), Times.Never());
        }

        [Test]
        public void RemoveCommentAndItsNotificationsIfEverythingIsFine_OnRemoveComment()
        {
            //Arrange
            var commentId = 12;
            var userId = 123;

            var comment = new Comment
                          {
                              Id = commentId,
                              UserId = userId,
                          };

            _uow.Setup(x => x.CommentRepository.GetById(commentId))
                .Returns(() => comment);
            _notificationService.Setup(x => x.RemoveCommentNotifications(commentId));

            //Act
            _sut.RemoveComment(commentId, userId);

            //Assert
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _notificationService.Verify(x => x.RemoveCommentNotifications(commentId), Times.Once());
        }

        [Test]
        public void CallRemovePostCommentsOnRepositoryAndRemoveAllNotifications_OnRemovePostComments()
        {
            //Arrange
            var commentId1 = 1;
            var commentId2 = 2;

            var commentIds = new List<int> { commentId1, commentId2 };
            var postId = 1;
            _uow.Setup(x => x.CommentRepository.RemovePostComments(postId))
                .Returns(commentIds);

            _notificationService.Setup(x => x.RemoveCommentNotifications(It.IsAny<int>()));

            //Act
            _sut.RemovePostComments(postId);

            //Assert
            _uow.Verify(x => x.CommentRepository.RemovePostComments(postId), Times.Once());

            _notificationService.Verify(x => x.RemoveCommentNotifications(It.IsAny<int>()),
                                        Times.Exactly(commentIds.Count));
            _notificationService.Verify(x => x.RemoveCommentNotifications(commentId1), Times.Once());
            _notificationService.Verify(x => x.RemoveCommentNotifications(commentId2), Times.Once());

            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CallRemoveEventCommentsOnRepositoryAndRemoveAllNotifications_OnRemoveEventComments()
        {
            //Arrange
            var commentId1 = 1;
            var commentId2 = 2;

            var commentIds = new List<int> { commentId1, commentId2 };
            var eventId = 1;
            _uow.Setup(x => x.CommentRepository.RemoveEventComments(eventId))
                .Returns(commentIds);

            _notificationService.Setup(x => x.RemoveCommentNotifications(It.IsAny<int>()));

            //Act
            _sut.RemoveEventComments(eventId);

            //Assert
            _uow.Verify(x => x.CommentRepository.RemoveEventComments(eventId), Times.Once());

            _notificationService.Verify(x => x.RemoveCommentNotifications(It.IsAny<int>()),
                                        Times.Exactly(commentIds.Count));
            _notificationService.Verify(x => x.RemoveCommentNotifications(commentId1), Times.Once());
            _notificationService.Verify(x => x.RemoveCommentNotifications(commentId2), Times.Once());

            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}