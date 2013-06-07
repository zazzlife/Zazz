using System;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Infrastructure.Services;

namespace Zazz.UnitTests.Infrastructure.Services
{
    [TestFixture]
    public class VoteServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private VoteService _sut;
        private int _photoId;
        private int _currentUserId;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _sut = new VoteService(_uow.Object);

            _photoId = 12;
            _currentUserId = 13;
        }

        [Test]
        public void ThrowIfPhotoIdIs0_OnAddPhotoVote()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.AddPhotoVote(0, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIdIs0_OnAddPhotoVote()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.AddPhotoVote(_photoId, 0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundIfPhotoNotExists_OnAddPhotoVote()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.Exists(_photoId))
                .Returns(false);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.AddPhotoVote(_photoId, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }


    }
}