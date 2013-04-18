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

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new PostService(_uow.Object);

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
            _uow.Verify(x => x.CommentRepository.RemovePostComments(_post.Id), Times.Never());
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
            _uow.Verify(x => x.CommentRepository.RemovePostComments(_post.Id), Times.Never());
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
            _uow.Setup(x => x.CommentRepository.RemovePostComments(_post.Id));

            //Act
            _sut.RemovePost(_post.Id, _post.FromUserId);

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(_post.Id), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemovePostComments(_post.Id), Times.Once());
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
            _uow.Setup(x => x.CommentRepository.RemovePostComments(post.Id));

            //Act
            _sut.RemovePost(post.Id, post.ToUserId.Value);

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(post), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeeds(post.Id), Times.Once());
            _uow.Verify(x => x.CommentRepository.RemovePostComments(post.Id), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
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