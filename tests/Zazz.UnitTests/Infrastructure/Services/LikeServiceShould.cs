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
    public class LikeServiceShould
    {
        private MockRepository _mockRepo;
        private Mock<IUoW> _uow;
        private LikeService _sut;
        private int _photoId;
        private int _currentUserId;
        private PhotoMinimalDTO _photo;

        [SetUp]
        public void Init()
        {
            _mockRepo = new MockRepository(MockBehavior.Strict);
            _uow = _mockRepo.Create<IUoW>();
            _sut = new LikeService(_uow.Object);

            _photo = new PhotoMinimalDTO
                     {
                         UserId = 4566,
                         Id = _photoId
                     };

            _photoId = 12;
            _currentUserId = 13;
        }

        #region GET_PHOTO_LIKES_COUNT

        [Test]
        public void ThrowIfPhotoIdIs0_OnGetPhotoLikesCount()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GetPhotoLikesCount(0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void GetValueFromRepository_OnGetPhotoLikesCount()
        {
            //Arrange
            var likesCount = 444;
            _uow.Setup(x => x.PhotoLikeRepository.GetLikesCount(_photoId))
                .Returns(likesCount);

            //Act
            var result = _sut.GetPhotoLikesCount(_photoId);

            //Assert
            Assert.AreEqual(likesCount, result);
            _mockRepo.VerifyAll();
        }

        #endregion

        #region PHOTO_LIKE_EXISTS

        [Test]
        public void ThrowIfPhotoIdIs0_OnPhotoLikeExists()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.PhotoLikeExists(0, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIdIs0_OnPhotoLikeExists()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.PhotoLikeExists(_photoId, 0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ReturnValueFromRepository_OnPhotoLikeExists()
        {
            //Arrange
            _uow.Setup(x => x.PhotoLikeRepository.Exists(_photoId, _currentUserId))
                .Returns(true);

            //Act
            var result = _sut.PhotoLikeExists(_photoId, _currentUserId);

            //Assert
            Assert.IsTrue(result);
            _mockRepo.VerifyAll();
        }

        #endregion

        #region ADD_PHOTO_LIKE

        [Test]
        public void ThrowIfPhotoIdIs0_OnAddPhotoLike()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.AddPhotoLike(0, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIdIs0_OnAddPhotoLike()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.AddPhotoLike(_photoId, 0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowNotFoundIfPhotoNotExists_OnAddPhotoLike()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(() => null);

            //Act
            Assert.Throws<NotFoundException>(() => _sut.AddPhotoLike(_photoId, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserHasAlreadyLiked_OnAddPhotoLike()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoLikeRepository.Exists(_photoId, _currentUserId))
                .Returns(true);

            //Act
            Assert.Throws<AlreadyLikedException>(() => _sut.AddPhotoLike(_photoId, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void AddNewLikeRecordAndIncrementLikeCounts_OnAddPhotoLike()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoLikeRepository.Exists(_photoId, _currentUserId))
                .Returns(false);

            _uow.Setup(x => x.PhotoLikeRepository.InsertGraph(It.Is<PhotoLike>(p => p.PhotoId == _photoId &&
                                                                                    p.UserId == _currentUserId)));
            
            _uow.Setup(x => x.UserReceivedLikesRepository.Increment(_photo.UserId));

            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.AddPhotoLike(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        #endregion

        #region REMOVE_PHOTO_LIKE

        [Test]
        public void ThrowIfPhotoIdIs0_OnRemovePhotoLike()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.RemovePhotoLike(0, _currentUserId));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void ThrowIfUserIdIs0_OnRemovePhotoLike()
        {
            //Arrange
            //Act
            Assert.Throws<ArgumentOutOfRangeException>(() => _sut.RemovePhotoLike(_photoId, 0));

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfPhotoNotExists_OnAddPhotoLike()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(() => null);

            //Act
            _sut.RemovePhotoLike(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void NotDoAnythingIfUserHasntLiked_OnAddPhotoLike()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoLikeRepository.Exists(_photoId, _currentUserId))
                .Returns(false);

            //Act
            _sut.RemovePhotoLike(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        [Test]
        public void RemoveLikeAndDecrementLikeCounts_OnAddPhotoLike()
        {
            //Arrange
            _uow.Setup(x => x.PhotoRepository.GetPhotoWithMinimalData(_photoId))
                .Returns(_photo);

            _uow.Setup(x => x.PhotoLikeRepository.Exists(_photoId, _currentUserId))
                .Returns(true);

            _uow.Setup(x => x.PhotoLikeRepository.Remove(_photoId, _currentUserId));
            _uow.Setup(x => x.UserReceivedLikesRepository.Decrement(_photo.UserId));
            _uow.Setup(x => x.SaveChanges());

            //Act
            _sut.RemovePhotoLike(_photoId, _currentUserId);

            //Assert
            _mockRepo.VerifyAll();
        }

        #endregion
    }
}