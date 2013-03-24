using System;
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
    }
}