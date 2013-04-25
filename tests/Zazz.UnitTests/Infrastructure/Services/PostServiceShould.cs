using System;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class PostServiceShould
    {
        private Mock<IUoW> _uow;
        private PostService _sut;
        private Post _post;
        private Mock<INotificationService> _notificationService;
        private Mock<ICommentService> _commentService;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _notificationService = new Mock<INotificationService>();
            _commentService = new Mock<ICommentService>();
            _sut = new PostService(_uow.Object, _notificationService.Object, _commentService.Object);

            _uow.Setup(x => x.SaveChanges());

            _post = new Post
                    {
                        Id = 1234,
                        CreatedTime = DateTime.MinValue,
                        Message = "message",
                        FromUserId = 12
                    };
        }

        [Test]
        public void CreateNewPostAndFeedAndShouldNotSetTimeHere_OnNewPost()
        // we should not set created time here because facebook gives us its own time.
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.InsertGraph(_post));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.NewPost(_post);

            //Assert
            Assert.AreEqual(DateTime.MinValue, _post.CreatedTime);
            _uow.Verify(x => x.PostRepository.InsertGraph(_post), Times.Once());
            _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void NotCreateNotificationIfPostIsOnHisOwnWall_OnNewPost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.InsertGraph(_post));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.NewPost(_post);


            //Assert
            _notificationService.Verify(x => x.CreateWallPostNotification(It.IsAny<int>(), It.IsAny<int>(), false),
                                        Times.Never());
        }

        [Test]
        public void CreateNotificationIfPostIsSomeoneElsesWall_OnNewPost()
        {
            //Arrange
            _post.ToUserId = 9983;
            _uow.Setup(x => x.PostRepository.InsertGraph(_post));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _notificationService.Setup(x => x.CreateWallPostNotification(_post.FromUserId,
                                                                         _post.ToUserId.Value, false));

            //Act
            _sut.NewPost(_post);


            //Assert
            _notificationService.Verify(x => x.CreateWallPostNotification(_post.FromUserId, _post.ToUserId.Value,
                                                                          false), Times.Once());
        }

        [Test]
        public void ThrowWhenCurrentUserIdIsNotSameAsPostOwner_OnRemovePost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);

            //Act
            try
            {
                _sut.RemovePost(_post.Id, 1);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            { }

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(_post.Id), Times.Never());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Never());
            _commentService.Verify(x => x.RemovePostComments(_post.Id), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void ThrowWhenCurrentUserIdIsNotSameAsPostTarget_OnRemovePost()
        {
            //Arrange
            _post.ToUserId = 12345;
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);

            //Act
            try
            {
                _sut.RemovePost(_post.Id, 1);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            { }

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(_post.Id), Times.Never());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Never());
            _commentService.Verify(x => x.RemovePostComments(_post.Id), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void RemoveAndPostAndSaveWhenEverythingIsFine_OnRemove()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);
            _uow.Setup(x => x.PostRepository.Remove(_post));
            _uow.Setup(x => x.FeedRepository.RemovePostFeeds(_post.Id));
            _commentService.Setup(x => x.RemovePostComments(_post.Id));

            //Act
            _sut.RemovePost(_post.Id, _post.FromUserId);

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(_post.Id), Times.Once());
            _commentService.Verify(x => x.RemovePostComments(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void AllowUserToRemovePostsIfTherAreNotTheOwnerButThePostIsOnTheirWall_OnRemovePost()
        {
            //Arrange
            var post = new Post { FromUserId = 12, Id = 1, ToUserId = 333 };
            _uow.Setup(x => x.PostRepository.GetById(post.Id))
                .Returns(post);
            _uow.Setup(x => x.PostRepository.Remove(post));
            _uow.Setup(x => x.FeedRepository.RemovePostFeeds(post.Id));
            _commentService.Setup(x => x.RemovePostComments(post.Id));

            //Act
            _sut.RemovePost(post.Id, post.ToUserId.Value);

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(post), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(post.Id), Times.Once());
            _commentService.Verify(x => x.RemovePostComments(post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void CallRemovePostNotifications_OnRemovePost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);
            _uow.Setup(x => x.PostRepository.Remove(_post));
            _uow.Setup(x => x.FeedRepository.RemovePostFeeds(_post.Id));
            _notificationService.Setup(x => x.RemovePostNotifications(_post.Id));

            //Act
            _sut.RemovePost(_post.Id, _post.FromUserId);

            //Assert
            _notificationService.Verify(x => x.RemovePostNotifications(_post.Id), Times.Once());
        }

        [Test]
        public void ThrowWhenPostNotExists_OnEditPost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(() => null);

            //Act
            try
            {
                _sut.EditPost(_post.Id, "new text", _post.FromUserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception)
            {
            }

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void ThrowWhenUserIdIsDifferent_OnEditPost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);

            //Act
            try
            {
                _sut.EditPost(_post.Id, "new text", 99);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void TargetUserShouldNotBeAbleToEditPost_OnEditPost()
        {
            //Arrange
            _post.ToUserId = 123;
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);

            //Act
            try
            {
                _sut.EditPost(_post.Id, "new text", _post.ToUserId.Value);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void SaveNewChanges_OnEditPost()
        {
            //Arrange
            var newText = "Edited";
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);

            //Act
            _sut.EditPost(_post.Id, newText, _post.FromUserId);

            //Assert
            Assert.AreEqual(newText, _post.Message);
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }
    }
}