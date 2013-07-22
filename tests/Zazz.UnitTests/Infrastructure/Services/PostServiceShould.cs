using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
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
        private Mock<IStringHelper> _stringHelper;
        private Mock<IStaticDataRepository> _staticDataRepo;
        private MockRepository _mockRepo;
        private List<Category> _categories;

        [SetUp]
        public void Init()
        {

            _mockRepo = new MockRepository(MockBehavior.Strict);

            _uow = _mockRepo.Create<IUoW>();
            _notificationService = _mockRepo.Create<INotificationService>();
            _stringHelper = _mockRepo.Create<IStringHelper>();
            _staticDataRepo = _mockRepo.Create<IStaticDataRepository>();

            _sut = new PostService(_uow.Object, _notificationService.Object,
                _stringHelper.Object, _staticDataRepo.Object);

            _uow.Setup(x => x.SaveChanges());

            _post = new Post
                    {
                        Id = 1234,
                        CreatedTime = DateTime.MinValue,
                        Message = "message",
                        FromUserId = 12
                    };

            _categories = new List<Category>
                          {
                              new Category
                              {
                                  Id = 1
                              },
                              new Category
                              {
                                  Id = 2
                              },
                              new Category
                              {
                                  Id = 3
                              },
                              new Category
                              {
                                  Id = 4
                              },
                              new Category
                              {
                                  Id = 5
                              },
                          };
        }

        [Test]
        public void ThrowIfPostWasNotFount_OnGetPost()
        {
            //Arrange
            var id = 444;
            _uow.Setup(x => x.PostRepository.GetById(id))
                .Returns(() => null);

            //Act & Assert
            Assert.Throws<NotFoundException>(() => _sut.GetPost(id));
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

            //Act
            _sut.NewPost(_post, Enumerable.Empty<int>());

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
            _sut.NewPost(_post, Enumerable.Empty<int>());


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
            _sut.NewPost(_post, Enumerable.Empty <int>());


            //Assert
            _notificationService.Verify(
                x => x.CreateWallPostNotification(_post.FromUserId, _post.ToUserId.Value, _post.Id,
                                                  false), Times.Once());
        }

        [Test]
        public void AddCategoriesIfAvailable_OnNewPost()
        {
            //Arrange
            var categories = new List<Category>
                             {
                                 new Category
                                 {
                                     Id = 1
                                 },
                                 new Category
                                 {
                                     Id = 2
                                 },
                                 new Category
                                 {
                                     Id = 3
                                 }
                             };

            _staticDataRepo.Setup(x => x.GetCategories())
                           .Returns(categories);

            _uow.Setup(x => x.PostRepository
                             .InsertGraph(It.Is<Post>(p =>
                                                      p.Categories.Any(c => c.CategoryId == 1) &&
                                                      p.Categories.Any(c => c.CategoryId == 2) &&
                                                      p.Categories.Any(c => c.CategoryId == 3) &&
                                                      p.Categories.Count == 3)));

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            

            //Act
            _sut.NewPost(_post, new int[] { 1, 2, 3 });

            //Assert
            _uow.Verify(x => x.PostRepository
                             .InsertGraph(It.Is<Post>(p =>
                                                      p.Categories.Any(c => c.CategoryId == 1) &&
                                                      p.Categories.Any(c => c.CategoryId == 2) &&
                                                      p.Categories.Any(c => c.CategoryId == 3) &&
                                                      p.Categories.Count == 3)), Times.Once());
        }

        [Test]
        public void NotAddInvalidCategories_OnNewPost()
        {
            //Arrange
            var categories = new List<Category>
                             {
                                 new Category
                                 {
                                     Id = 1
                                 },
                                 new Category
                                 {
                                     Id = 2
                                 },
                                 new Category
                                 {
                                     Id = 3
                                 }
                             };

            _staticDataRepo.Setup(x => x.GetCategories())
                           .Returns(categories);

            _uow.Setup(x => x.PostRepository
                             .InsertGraph(It.Is<Post>(p =>
                                                      p.Categories.Any(c => c.CategoryId == 1) &&
                                                      p.Categories.Any(c => c.CategoryId == 2) &&
                                                      p.Categories.Any(c => c.CategoryId == 3) &&
                                                      p.Categories.Count == 3)));

            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));



            //Act
            _sut.NewPost(_post, new int[] { 1, 2, 3, 4, 5, 6 });

            //Assert
            _uow.Verify(x => x.PostRepository
                             .InsertGraph(It.Is<Post>(p =>
                                                      p.Categories.Any(c => c.CategoryId == 1) &&
                                                      p.Categories.Any(c => c.CategoryId == 2) &&
                                                      p.Categories.Any(c => c.CategoryId == 3) &&
                                                      p.Categories.Count == 3)), Times.Once());
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
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Never());
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
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Never());
            _uow.Verify(x => x.SaveChanges(), Times.Never());
        }

        [Test]
        public void RemoveAndPostAndSaveWhenEverythingIsFine_OnRemove()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(_post);
            _uow.Setup(x => x.PostRepository.Remove(_post));

            //Act
            _sut.RemovePost(_post.Id, _post.FromUserId);

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(_post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Once());
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

            //Act
            _sut.RemovePost(post.Id, post.ToUserId.Value);

            //Assert
            _uow.Verify(x => x.PostRepository.GetById(post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(post), Times.Once());
            _uow.Verify(x => x.SaveChanges(), Times.Once());
        }

        [Test]
        public void ThrowWhenPostNotExists_OnEditPost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetById(_post.Id))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() =>  _sut.EditPost(_post.Id, "new text", Enumerable.Empty<int>(), _post.FromUserId));

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
            Assert.Throws<SecurityException>(() => _sut.EditPost(_post.Id, "new text", Enumerable.Empty<int>(), 99));

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
                _sut.EditPost(_post.Id, "new text", Enumerable.Empty<int>(), _post.ToUserId.Value);
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
                           Categories = new List<PostCategory> { new PostCategory { CategoryId = 0 }, new PostCategory { CategoryId = 0 } },
                           Message = "m"
                       };

            var newText = "Edited ";

            _uow.Setup(x => x.PostRepository.GetById(post.Id))
                .Returns(post);

            //Act
            _sut.EditPost(post.Id, newText, Enumerable.Empty<int>(), post.FromUserId);

            //Assert
            Assert.AreEqual(newText, post.Message);
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ChangeCategories_OnEditPost()
        {
            //Arrange
            var post = new Post
            {
                Id = 17,
                FromUserId = 123,
                ToUserId = 1234,
                Categories = new List<PostCategory>
                             {
                                 new PostCategory { CategoryId = 1 }, new PostCategory { CategoryId = 2 }
                             },
                Message = "m"
            };

            var newText = "Edited ";
            

            _uow.Setup(x => x.PostRepository.GetById(post.Id))
                .Returns(post);

            _staticDataRepo.Setup(x => x.GetCategories())
                           .Returns(_categories);

            var categories = new[] {3, 4};
            
            //Act
            _sut.EditPost(post.Id, newText, categories, post.FromUserId);

            //Assert
            Assert.IsFalse(post.Categories.Any(c => c.CategoryId == 1));
            Assert.IsFalse(post.Categories.Any(c => c.CategoryId == 2));
            Assert.IsTrue(post.Categories.Any(c => c.CategoryId == 3));
            Assert.IsTrue(post.Categories.Any(c => c.CategoryId == 4));
            Assert.AreEqual(2, post.Categories.Count);
            _mockRepo.VerifyAll();
        }
    }
}