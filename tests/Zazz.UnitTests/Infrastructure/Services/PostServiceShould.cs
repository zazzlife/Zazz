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
        private int _userId;
        private Post _post;

        [SetUp]
        public void Init()
        {
            _uow = new Mock<IUoW>();
            _sut = new PostService(_uow.Object);
            _userId = 21;
            _post = new Post {UserId = _userId};

            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));
        }

        [Test]
        public async Task ThrowExceptionWhenUserIdIs0_OnCreateEvent()
        {
            //Arrange
            _post.UserId = 0;
            //Act
            try
            {
                await _sut.CreatePostAsync(_post);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException e)
            {
                //Assert
            }
        }

        [Test]
        public async Task InsertAndSaveAndCreateFeed_OnCreateEvent()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.InsertGraph(_post));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            await _sut.CreatePostAsync(_post);

            //Assert
            _uow.Verify(x => x.PostRepository.InsertGraph(_post), Times.Once());
            _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Exactly(2));
        }

        [Test]
        public async Task CallGetById_OnGetPost()
        {
            //Arrange
            var id = 123;
            var post = new Post();
            _uow.Setup(x => x.PostRepository.GetByIdAsync(id))
                .Returns(() => Task.Run(() => post));

            //Act
            var result = await _sut.GetPostAsync(id);

            //Assert
            Assert.AreSame(post, result);
            _uow.Verify(x => x.PostRepository.GetByIdAsync(id), Times.Once());
        }

        [Test]
        public async Task ThrownIfEventIdIs0_OnUpdateEvent()
        {
            //Arrange
            //Act
            try
            {
                await _sut.UpdatePostAsync(_post, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

        }

        [Test]
        public async Task ThrowIfCurrentUserDoesntMatchTheOwner_OnUpdateEvent()
        {
            //Arrange
            _post.Id = 444;
            _uow.Setup(x => x.PostRepository.GetOwnerIdAsync(_post.Id))
                .Returns(() => Task.Run(() => 123));

            //Act

            try
            {
                await _sut.UpdatePostAsync(_post, _userId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PostRepository.GetOwnerIdAsync(_post.Id), Times.Once());
        }

        [Test]
        public async Task SaveUpdatedEvent_OnUpdateEvent()
        {
            //Arrange
            _post.Id = 444;
            _uow.Setup(x => x.PostRepository.GetOwnerIdAsync(_post.Id))
                .Returns(() => Task.Run(() => _post.UserId));

            //Act
            await _sut.UpdatePostAsync(_post, _userId);

            //Assert
            _uow.Verify(x => x.PostRepository.InsertOrUpdate(_post), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());

        }

        [Test]
        public async Task ShouldThrowIfEventIdIs0_OnDelete()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetOwnerIdAsync(_post.Id))
                .Returns(() => Task.Run(() => 123));

            //Act
            try
            {
                await _sut.DeletePostAsync(0, _post.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (ArgumentException)
            {
            }

            //Assert

        }

        [Test]
        public async Task ThrowIfUserIdDoesntMatchTheOwnerId_OnDelete()
        {
            //Arrange
            _post.Id = 444;
            _uow.Setup(x => x.PostRepository.GetOwnerIdAsync(_post.Id))
                .Returns(() => Task.Run(() => 123));

            //Act

            try
            {
                await _sut.DeletePostAsync(_post.Id, _post.UserId);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {
            }

            //Assert
            _uow.Verify(x => x.PostRepository.GetOwnerIdAsync(_post.Id), Times.Once());
        }

        [Test]
        public async Task Delete_OnDelete()
        {
            //Arrange
            _post.Id = 444;
            _uow.Setup(x => x.PostRepository.GetOwnerIdAsync(_post.Id))
                .Returns(() => Task.Run(() =>_post.UserId));
            _uow.Setup(x => x.PostRepository.RemoveAsync(_post.Id))
                .Returns(() => Task.Run(() => { }));
            _uow.Setup(x => x.FeedRepository.RemovePostFeed(_post.Id));

            //Act
            await _sut.DeletePostAsync(_post.Id, _userId);

            //Assert
            _uow.Verify(x => x.PostRepository.RemoveAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
            _uow.Verify(x => x.FeedRepository.RemovePostFeed(_post.Id), Times.Once());
        }


    }
}