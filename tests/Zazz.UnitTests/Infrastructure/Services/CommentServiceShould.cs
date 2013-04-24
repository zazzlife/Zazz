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

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _notificationService = new Mock<INotificationService>();
            _sut = new CommentService(_uow.Object, _notificationService.Object);

            _uow.Setup(x => x.SaveChanges());
        }

        [Test]
        public void CreateNotificationAndSaveCommentWhenCommentIsOnPhoto_OnCreateComment()
        {
            //Arrange
            var ownerId = 444;
            var comment = new Comment
                          {
                              FromId = 1,
                              PhotoId = 2,
                              PostId = 3,
                              EventId = 4
                          };

            var photo = new Photo
                        {
                            UserId = ownerId
                        };

            _uow.Setup(x => x.CommentRepository.InsertGraph(It.IsAny<Comment>()));
            _uow.Setup(x => x.PhotoRepository.GetById(comment.PhotoId.Value))
                .Returns(photo);

            _notificationService.Setup(x => x.CreatePhotoCommentNotification(comment.PhotoId.Value, ownerId, false));

            //Act
            _sut.CreateComment(comment, CommentType.Photo);

            //Assert
            _uow.Verify(x => x.CommentRepository.InsertGraph(It.IsAny<Comment>()), Times.Once());
            _uow.Verify(x => x.PhotoRepository.GetById(comment.PhotoId.Value), Times.Once());
            _notificationService.Verify(x => x.CreatePhotoCommentNotification(comment.PhotoId.Value, ownerId, false),
                                        Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}