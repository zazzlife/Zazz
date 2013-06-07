using System;
using Moq;
using NUnit.Framework;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;
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
        private PhotoMinimalDTO _photo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _sut = new VoteService(_uow.Object);

            _photo = new PhotoMinimalDTO
                     {
                         UserId = 4566,
                         Id = _photoId
                     };

            _photoId = 12;
            _currentUserId = 13;
        }

        #region GET_PHOTO_VOTES_COUNT

        [Test]
        public void ThrowIfPhotoIdIs0_OnGetPhotoVotesCount()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetPhotoVotesCount(0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void GetValueFromRepository_OnGetPhotoVotesCount()
        {
            //Arrange
            var votesCount = 444;
            _uow.Setup(x => x.PhotoVoteRepository.GetVotesCount(_photoId))
                .Returns(votesCount);

            //Act
            var result = _sut.GetPhotoVotesCount(_photoId);

            //Assert
            Assert.AreEqual(votesCount, result);
            _mockRepo.VerifyAll();
        }

        #endregion

        #region HAS_USER_VOTED_ON_PHOTO

        [Test]
        public void ThrowIfPhotoIdIs0_OnHasUserVotedOnPhoto()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.HasUserVotedOnPhoto(0, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIdIs0_OnHasUserVotedOnPhoto()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.HasUserVotedOnPhoto(_photoId, 0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnValueFromRepository_OnHasUserVotedOnPhoto()
        {
            //Arrange
            _uow.Setup(x => x.PhotoVoteRepository.Exists(_photoId, _currentUserId))
                .Returns(true);

            //Act
            var result = _sut.HasUserVotedOnPhoto(_photoId, _currentUserId);

            //Assert
            Assert.IsTrue(result);
            _mockRepo.VerifyAll();
        }

        #endregion

        #region ADD_PHOTO_VOTE

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
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.AddPhotoVote(_photoId, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserHasAlreadyVoted_OnAddPhotoVote()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoVoteRepository.Exists(_photoId, _currentUserId))
                .Returns(true);

            //Act
            Assert.Throws<AlreadyVotedException>(() => _sut.AddPhotoVote(_photoId, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddNewVoteRecordAndIncrementVoteCounts_OnAddPhotoVote()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoVoteRepository.Exists(_photoId, _currentUserId))
                .Returns(false);

            _uow.Setup(x => x.PhotoVoteRepository.InsertGraph(It.Is<PhotoVote>(p => p.PhotoId == _photoId &&
                                                                                    p.UserId == _currentUserId)));
            
            _uow.Setup(x => x.UserReceivedVotesRepository.Increment(_photo.UserId));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddPhotoVote(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        #endregion

        #region REMOVE_PHOTO_VOTE

        [Test]
        public void ThrowIfPhotoIdIs0_OnRemovePhotoVote()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.RemovePhotoVote(0, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIdIs0_OnRemovePhotoVote()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.RemovePhotoVote(_photoId, 0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfPhotoNotExists_OnAddPhotoVote()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(() => null);

            //Act
            _sut.RemovePhotoVote(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfUserHasntVoted_OnAddPhotoVote()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoVoteRepository.Exists(_photoId, _currentUserId))
                .Returns(false);

            //Act
            _sut.RemovePhotoVote(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveVoteAndDecrementVoteCounts_OnAddPhotoVote()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoVoteRepository.Exists(_photoId, _currentUserId))
                .Returns(true);

            _uow.Setup(x => x.PhotoVoteRepository.Remove(_photoId, _currentUserId));
            _uow.Setup(x => x.UserReceivedVotesRepository.Decrement(_photo.UserId));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemovePhotoVote(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        #endregion
    }
}