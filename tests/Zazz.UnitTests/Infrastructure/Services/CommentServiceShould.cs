using System.Collections.Generic;
using System.Security;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
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
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);

            _uow = _mockRepo.Create<IUoW>();
            _notificationService = _mockRepo.Create<INotificationService>();

            _sut = new CommentService(_uow.Object, _notificationService.Object);

            _ownerId = 444;
            _comment = new Comment
            {
                Id = 12345,
                UserId = 1,
                PhotoComment = new PhotoComment {PhotoId = 2},
                PostComment = new PostComment {PostId = 3},
                EventComment = new EventComment {EventId = 4}
            };
        }

        [Test]
        public void ThrowNotFoundWhenPhotoDoesNotExists_OnCreateComment()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetById(_comment.PhotoComment.PhotoId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.CreateComment(_comment, CommentType.Photo));

            //Assert
            _mockRepo.VerifyAll();
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

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Photo);

            //Assert
            _mockRepo.VerifyAll();
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

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Photo);


            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundWhenPostDoesNotExists_OnCreateComment()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostComment.PostId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.CreateComment(_comment, CommentType.Post));

            //Assert
            _mockRepo.VerifyAll();
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
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostComment.PostId))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId, _comment.PostComment.PostId, _ownerId, false));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _mockRepo.VerifyAll();
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
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostComment.PostId))
                .Returns(post);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotifyOwnerIfTheCommenterIsSomeoneElse_OnCreateComment()
        {
            //Arrange
            var post = new Post
                       {
                           Id = _comment.PostComment.PostId,
                           FromUserId = _ownerId
                       };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostComment.PostId))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId, _comment.PostComment.PostId, _ownerId, false));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotifyBothFROM_USERAndTO_USERIfTheCommenterIsSomeoneElse_OnCreateComment()
        {
            //Arrange
            var post = new Post
            {
                Id = _comment.PostComment.PostId,
                FromUserId = _ownerId,
                ToUserId = 888
            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostComment.PostId))
                .Returns(post);

            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                _comment.PostComment.PostId, post.FromUserId, false));

            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                _comment.PostComment.PostId, post.ToUserId.Value, false));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void OnlyNotifyTO_USERIfCommentIsFromFROM_USER_OnCreateComment()
        {
            //Arrange
            var post = new Post
            {
                Id = _comment.PostComment.PostId,
                FromUserId = _comment.UserId,
                ToUserId = 888
            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostComment.PostId))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                _comment.PostComment.PostId, post.ToUserId.Value, false));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void OnlyNotifyFROM_USERIfCommentIsFromTO_USER_OnCreateComment()
        {
            //Arrange
            var post = new Post
            {
                Id = _comment.PostComment.PostId,
                FromUserId = _ownerId,
                ToUserId = _comment.UserId
            };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostComment.PostId))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.Id, _comment.UserId,
                _comment.PostComment.PostId, post.FromUserId, false));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundWhenEventDoesNotExists_OnCreateComment()
        {
            //Arrange
            _uow.Setup(x => x.EventRepository.GetById(_comment.EventComment.EventId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.CreateComment(_comment, CommentType.Event));

            //Assert
            _mockRepo.VerifyAll();
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
            _uow.Setup(x => x.EventRepository.GetById(_comment.EventComment.EventId))
                .Returns(zazzEvent);
            _notificationService.Setup(x => x.CreateEventCommentNotification(_comment.Id, _comment.UserId, _comment.EventComment.EventId, _ownerId, false));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Event);

            //Assert
            _mockRepo.VerifyAll();
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
            _uow.Setup(x => x.EventRepository.GetById(_comment.EventComment.EventId))
                .Returns(zazzEvent);

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.CreateComment(_comment, CommentType.Event);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundIfCommentDoesntExists_OnEditComment()
        {
            //Arrange
            var commentId = 12;
            _uow.Setup(x => x.CommentRepository.GetById(commentId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.EditComment(commentId, 123, ""));

            //Assert
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.EditComment(commentId, userId, newMessage);

            //Assert
            Assert.AreEqual(newMessage, comment.Message);
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
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
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveCommentIfEverythingIsFine_OnRemoveComment()
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

            _uow.Setup(x => x.CommentRepository.Remove(comment));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemoveComment(commentId, userId);

            //Assert
            _mockRepo.VerifyAll();
        }
    }
}