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
                FromId = 1,
                PhotoId = 2,
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
            _uow.Setup(x => x.PhotoRepository.GetById(_comment.PhotoId.Value))
                .Returns(photo);

            _notificationService.Setup(x => x.CreatePhotoCommentNotification(_comment.PhotoId.Value, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Photo);

            //Assert
            _uow.Verify(x => x.CommentRepository.InsertGraph(_comment), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetById(_comment.PhotoId.Value), Times.Once());
            _notificationService.Verify(x => x.CreatePhotoCommentNotification(_comment.PhotoId.Value, _ownerId, false),
                                        Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CreateNotificationAndSaveCommentWhenCommentIsOnPost_OnCreateComment()
        {
            //Arrange

            var post = new Post
                       {
                           FromUserId = _ownerId
                       };

            _uow.Setup(x => x.CommentRepository.InsertGraph(_comment));
            _uow.Setup(x => x.PostRepository.GetById(_comment.PostId.Value))
                .Returns(post);
            _notificationService.Setup(x => x.CreatePostCommentNotification(_comment.PostId.Value, _ownerId, false));

            //Act
            _sut.CreateComment(_comment, CommentType.Post);

            //Assert
            _uow.Verify(x => x.CommentRepository.InsertGraph(_comment), Times.Once());
            _uow.Verify(x => x.PostRepository.GetById(_comment.PostId.Value), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
            _notificationService.Verify(x => x.CreatePostCommentNotification(_comment.PostId.Value, _ownerId, false)
                , Times.Once());
        }
    }
}