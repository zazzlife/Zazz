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

            _uow.Setup(x => x.SaveAsync())
                .Returns(() => Task.Run(() => { }));

            _post = new Post
                    {
                        Id = 1234,
                        CreatedTime = DateTime.MinValue,
                        Message = "message",
                        UserId = 12
                    };
        }

        [Test]
        public async Task CreateNewPostAndFeedAndShouldNotSetTimeHere_OnNewPost() 
            // we should not set created time here because facebook gives us its own time.
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.InsertGraph(_post));
            _uow.Setup(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()));

            //Act
            await _sut.NewPostAsync(_post);

            //Assert
            Assert.AreEqual(DateTime.MinValue, _post.CreatedTime);
            _uow.Verify(x => x.PostRepository.InsertGraph(_post), Times.Once());
            _uow.Verify(x => x.FeedRepository.InsertGraph(It.IsAny<Feed>()), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());

        }

        [Test]
        public async Task ThrowWhenCurrentUserIdIsNotSameAsPostOwner_OnRemovePost()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetByIdAsync(_post.Id))
                .Returns(() => Task.Run(() => _post));

            //Act
            try
            {
                await _sut.RemovePostAsync(_post.Id, 1);
                Assert.Fail("Expected exception was not thrown");
            }
            catch (SecurityException)
            {}
            
            //Assert
            _uow.Verify(x => x.PostRepository.GetByIdAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Never());
            _uow.Verify(x => x.SaveAsync(), Times.Never());
        }

        [Test]
        public async Task RemoveAndPostAndSaveWhenEverythingIsFine_OnRemove()
        {
            //Arrange
            _uow.Setup(x => x.PostRepository.GetByIdAsync(_post.Id))
                .Returns(() => Task.Run(() => _post));
            _uow.Setup(x => x.PostRepository.Remove(_post));

            //Act
            await _sut.RemovePostAsync(_post.Id, _post.UserId);

            //Assert
            _uow.Verify(x => x.PostRepository.GetByIdAsync(_post.Id), Times.Once());
            _uow.Verify(x => x.PostRepository.Remove(_post), Times.Once());
            _uow.Verify(x => x.SaveAsync(), Times.Once());
        }
    }
}