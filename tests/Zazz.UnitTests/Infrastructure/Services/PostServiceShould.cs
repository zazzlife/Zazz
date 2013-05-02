using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private Mock<IStringHelper> _stringHelper;
        private Mock<IStaticDataRepository> _staticDataRepo;
        private MockRepository _mockRepo;

        [SetUp]
        public void Init()
        {

            _mockRepo = new MockRepository(MockBehavior.Strict);

            _uow = _mockRepo.Create<IUoW>();
            _notificationService = _mockRepo.Create<INotificationService>();
            _commentService = _mockRepo.Create<ICommentService>();
            _stringHelper = _mockRepo.Create<IStringHelper>();
            _staticDataRepo = _mockRepo.Create<IStaticDataRepository>();

            _sut = new PostService(_uow.Object, _notificationService.Object, _commentService.Object,
                _stringHelper.Object, _staticDataRepo.Object);

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
        public void CallGetByIdOnRepository_OnGetPost()
        {
            //Arrange
            var id = 444;
            var post = new Post();
            _uow.Setup(x => x.PostRepository.GetById(id))
                .Returns(post);

            //Act
            var result = _sut.GetPost(id);

            //Assert
            Assert.AreSame(post, result);
            _uow.Verify(x => x.PostRepository.GetById(id), Times.Once());
        }

        [Test]
        public void CreateNewPostAndFeedAndShouldNotSetTimeHere_OnNewPost()
        // we should not set created time here because facebook gives us its own time.
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.InsertGraph(_post));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));
            _stringHelper.Setup(x => x.ExtractTags(_post.Message))
                         .Returns(Enumerable.Empty<string>);

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
            _stringHelper.Setup(x => x.ExtractTags(_post.Message))
                         .Returns(Enumerable.Empty<string>);

            //Act
            _sut.NewPost(_post);


            //Assert
            _notificationService.Verify(x => x.CreateWallPostNotification(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), false),
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
                                                                         _post.ToUserId.Value, _post.Id, false));
            _stringHelper.Setup(x => x.ExtractTags(_post.Message))
                         .Returns(Enumerable.Empty<string>);

            //Act
            _sut.NewPost(_post);


            //Assert
            _notificationService.Verify(
                x => x.CreateWallPostNotification(_post.FromUserId, _post.ToUserId.Value, _post.Id,
                                                  false), Times.Once());
        }

        [Test]
        public void AddTagsIfExists_OnNewPost()
        {
            //Arrange
            var tag1 = "#test1";
            var tag2 = "#test2";

            var tagObject1 = new Tag { Id = 1, Name = "test1" };
            var tagObject2 = new Tag { Id = 2, Name = "test2" };

            _stringHelper.Setup(x => x.ExtractTags(_post.Message))
                         .Returns(new[] { tag1, tag2 });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tagObject1);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tagObject2);

            _uow.Setup(x => x.PostRepository
                .InsertGraph(It.Is<Post>(post => (post.Tags.Count == 2
                    && post.Tags.Any(p => p.TagId == tagObject1.Id)
                    && post.Tags.Any(p => p.TagId == tagObject2.Id)))));

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.NewPost(_post);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotAddDuplicateTags_OnNewPost()
        {
            //Arrange
            var tag1 = "#test1";
            var duplicateTag1 = "#test1";
            var tag2 = "#test2";

            var tagObject1 = new Tag { Id = 1, Name = "test1" };
            var tagObject2 = new Tag { Id = 2, Name = "test2" };

            _stringHelper.Setup(x => x.ExtractTags(_post.Message))
                         .Returns(new[] { tag1, tag2, duplicateTag1 });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag1.Replace("#", "")))
                           .Returns(tagObject1);
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag2.Replace("#", "")))
                           .Returns(tagObject2);

            _uow.Setup(x => x.PostRepository
                .InsertGraph(It.Is<Post>(post => (post.Tags.Count == 2
                    && post.Tags.Any(p => p.TagId == tagObject1.Id)
                    && post.Tags.Any(p => p.TagId == tagObject2.Id)))));

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            _sut.NewPost(_post);

            //Assert
            _mockRepo.VerifyAll();
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
            _notificationService.Setup(x => x.RemovePostNotifications(_post.Id));

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
            _notificationService.Setup(x => x.RemovePostNotifications(post.Id));
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
            _commentService.Setup(x => x.RemovePostComments(_post.Id));

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
            var post = new Post
                       {
                           Id = 17,
                           FromUserId = 123,
                           ToUserId = 1234,
                           Tags = new List<PostTag> { new PostTag { TagId = 0 }, new PostTag { TagId = 0 } },
                           Message = "m"
                       };

            var tag = "#new-edit";
            var newText = "Edited " + tag;
            var tagObject = new Tag { Id = 3, Name = "new-edit" };

            _uow.Setup(x => x.PostRepository.GetById(post.Id))
                .Returns(post);
            _stringHelper.Setup(x => x.ExtractTags(newText))
                         .Returns(new[] { tag });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag.Replace("#", "")))
                           .Returns(tagObject);

            //Act
            _sut.EditPost(post.Id, newText, post.FromUserId);

            //Assert
            Assert.AreEqual(newText, post.Message);
            Assert.AreEqual(1, post.Tags.Count);
            Assert.IsTrue(post.Tags.Any(t => t.TagId == tagObject.Id));
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotSaveDuplicateTags_OnEditPost()
        {
            //Arrange
            var post = new Post
            {
                Id = 17,
                FromUserId = 123,
                ToUserId = 1234,
                Tags = new List<PostTag> { new PostTag { TagId = 0 }, new PostTag { TagId = 0 } },
                Message = "m"
            };

            var tag = "#new-edit";
            var duplicateTag = tag;
            var newText = tag + " Edited " + duplicateTag;
            var tagObject = new Tag { Id = 3, Name = "new-edit" };

            _uow.Setup(x => x.PostRepository.GetById(post.Id))
                .Returns(post);
            _stringHelper.Setup(x => x.ExtractTags(newText))
                         .Returns(new[] { tag, duplicateTag });
            _staticDataRepo.Setup(x => x.GetTagIfExists(tag.Replace("#", "")))
                           .Returns(tagObject);

            //Act
            _sut.EditPost(post.Id, newText, post.FromUserId);

            //Assert
            Assert.AreEqual(newText, post.Message);
            Assert.AreEqual(1, post.Tags.Count);
            Assert.IsTrue(post.Tags.Any(t => t.TagId == tagObject.Id));
            _mockRepo.VerifyAll();
        }
    }
}